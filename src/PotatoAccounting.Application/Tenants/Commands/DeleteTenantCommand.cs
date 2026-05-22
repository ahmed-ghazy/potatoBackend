using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Tenants.Commands;

public record DeleteTenantCommand(int Id) : IRequest;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteTenantCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), request.Id);

        _db.Tenants.Remove(tenant);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
