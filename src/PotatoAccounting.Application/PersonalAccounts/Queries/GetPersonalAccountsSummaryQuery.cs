using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.PersonalAccounts.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.PersonalAccounts.Queries;

public record GetPersonalAccountsSummaryQuery : IRequest<PersonalAccountsSummaryDto>;

public class GetPersonalAccountsSummaryQueryHandler : IRequestHandler<GetPersonalAccountsSummaryQuery, PersonalAccountsSummaryDto>
{
    private readonly IApplicationDbContext _db;

    public GetPersonalAccountsSummaryQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PersonalAccountsSummaryDto> Handle(GetPersonalAccountsSummaryQuery request, CancellationToken cancellationToken)
    {
        var accountsCount = await _db.PersonalAccounts.CountAsync(cancellationToken);

        var totalGiven = await _db.PersonalAccountEntries
            .Where(e => e.Direction == PersonalAccountEntryDirection.Given)
            .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;

        var totalReceived = await _db.PersonalAccountEntries
            .Where(e => e.Direction == PersonalAccountEntryDirection.Received)
            .SumAsync(e => (decimal?)(e.Amount - e.Fee), cancellationToken) ?? 0m;

        return new PersonalAccountsSummaryDto
        {
            AccountsCount = accountsCount,
            TotalGiven = totalGiven,
            TotalReceived = totalReceived,
        };
    }
}
