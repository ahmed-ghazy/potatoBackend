using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.PersonalAccounts.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.PersonalAccounts.Queries;

public record GetPersonalAccountByIdQuery(int Id) : IRequest<PersonalAccountDto>;

public class GetPersonalAccountByIdQueryHandler : IRequestHandler<GetPersonalAccountByIdQuery, PersonalAccountDto>
{
    private readonly IApplicationDbContext _db;

    public GetPersonalAccountByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PersonalAccountDto> Handle(GetPersonalAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _db.PersonalAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(PersonalAccount), request.Id);

        var totalGiven = await _db.PersonalAccountEntries
            .Where(e => e.PersonalAccountId == account.Id && e.Direction == PersonalAccountEntryDirection.Given)
            .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;
        var totalReceived = await _db.PersonalAccountEntries
            .Where(e => e.PersonalAccountId == account.Id && e.Direction == PersonalAccountEntryDirection.Received)
            .SumAsync(e => (decimal?)(e.Amount - e.Fee), cancellationToken) ?? 0m;
        var entriesCount = await _db.PersonalAccountEntries
            .CountAsync(e => e.PersonalAccountId == account.Id, cancellationToken);

        return new PersonalAccountDto
        {
            Id = account.Id,
            Name = account.Name,
            PhoneNumber = account.PhoneNumber,
            Notes = account.Notes,
            CreatedAt = account.CreatedAt,
            TotalGiven = totalGiven,
            TotalReceived = totalReceived,
            EntriesCount = entriesCount,
        };
    }
}
