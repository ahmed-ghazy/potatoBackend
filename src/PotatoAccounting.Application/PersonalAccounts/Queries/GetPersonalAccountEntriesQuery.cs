using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.PersonalAccounts.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.PersonalAccounts.Queries;

public record GetPersonalAccountEntriesQuery(int PersonalAccountId) : IRequest<IReadOnlyList<PersonalAccountEntryDto>>;

public class GetPersonalAccountEntriesQueryHandler : IRequestHandler<GetPersonalAccountEntriesQuery, IReadOnlyList<PersonalAccountEntryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPersonalAccountEntriesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<PersonalAccountEntryDto>> Handle(GetPersonalAccountEntriesQuery request, CancellationToken cancellationToken)
    {
        var accountExists = await _db.PersonalAccounts
            .AnyAsync(a => a.Id == request.PersonalAccountId, cancellationToken);
        if (!accountExists)
            throw new NotFoundException(nameof(PersonalAccount), request.PersonalAccountId);

        var entries = await _db.PersonalAccountEntries.AsNoTracking()
            .Where(e => e.PersonalAccountId == request.PersonalAccountId)
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.Id)
            .ToListAsync(cancellationToken);

        return entries.Select(PersonalAccountEntryDto.From).ToList();
    }
}
