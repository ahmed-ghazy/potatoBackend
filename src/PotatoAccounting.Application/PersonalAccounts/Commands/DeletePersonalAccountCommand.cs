using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.PersonalAccounts.Commands;

public record DeletePersonalAccountCommand(int Id) : IRequest;

public class DeletePersonalAccountCommandHandler : IRequestHandler<DeletePersonalAccountCommand>
{
    private readonly IApplicationDbContext _db;

    public DeletePersonalAccountCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeletePersonalAccountCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.PersonalAccounts.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(PersonalAccount), request.Id);

        _db.PersonalAccounts.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
