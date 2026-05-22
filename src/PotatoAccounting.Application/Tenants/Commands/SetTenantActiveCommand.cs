using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Tenants.Commands;

public record SetTenantActiveCommand(int Id, bool IsActive) : IRequest;

public class SetTenantActiveCommandHandler : IRequestHandler<SetTenantActiveCommand>
{
    private readonly IApplicationDbContext _db;

    public SetTenantActiveCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(SetTenantActiveCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.Id);

        if (request.IsActive) tenant.Activate();
        else tenant.Deactivate();

        await _db.SaveChangesAsync(cancellationToken);
    }
}
