using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.PersonalAccounts.Commands;

public record DeletePersonalAccountEntryCommand(int PersonalAccountId, int Id) : IRequest;

public class DeletePersonalAccountEntryCommandHandler : IRequestHandler<DeletePersonalAccountEntryCommand>
{
    private readonly IApplicationDbContext _db;

    public DeletePersonalAccountEntryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeletePersonalAccountEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _db.PersonalAccountEntries
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.PersonalAccountId == request.PersonalAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(PersonalAccountEntry), request.Id);

        _db.PersonalAccountEntries.Remove(entry);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
