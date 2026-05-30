using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.PersonalAccounts.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.PersonalAccounts.Queries;

public record GetAllPersonalAccountsQuery(string? Search = null) : IRequest<IReadOnlyList<PersonalAccountDto>>;

public class GetAllPersonalAccountsQueryHandler : IRequestHandler<GetAllPersonalAccountsQuery, IReadOnlyList<PersonalAccountDto>>
{
    private readonly IApplicationDbContext _db;

    public GetAllPersonalAccountsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<PersonalAccountDto>> Handle(GetAllPersonalAccountsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.PersonalAccounts.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(a =>
                EF.Functions.Like(a.Name, $"%{s}%")
                || (a.PhoneNumber != null && EF.Functions.Like(a.PhoneNumber, $"%{s}%")));
        }

        // Group entry totals by account in a single round-trip, then join in memory.
        // Received totals use net (Amount - Fee) so Vodafone Cash fees are reflected in the balance.
        var totals = await _db.PersonalAccountEntries.AsNoTracking()
            .GroupBy(e => e.PersonalAccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                TotalGiven = g.Where(e => e.Direction == PersonalAccountEntryDirection.Given).Sum(e => (decimal?)e.Amount) ?? 0m,
                TotalReceived = g.Where(e => e.Direction == PersonalAccountEntryDirection.Received).Sum(e => (decimal?)(e.Amount - e.Fee)) ?? 0m,
                Count = g.Count(),
            })
            .ToListAsync(cancellationToken);

        var totalsByAccount = totals.ToDictionary(t => t.AccountId);

        var accounts = await query
            .OrderBy(a => a.Name)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.PhoneNumber,
                a.Notes,
                a.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return accounts.Select(a =>
        {
            var t = totalsByAccount.GetValueOrDefault(a.Id);
            return new PersonalAccountDto
            {
                Id = a.Id,
                Name = a.Name,
                PhoneNumber = a.PhoneNumber,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                TotalGiven = t?.TotalGiven ?? 0m,
                TotalReceived = t?.TotalReceived ?? 0m,
                EntriesCount = t?.Count ?? 0,
            };
        }).ToList();
    }
}
