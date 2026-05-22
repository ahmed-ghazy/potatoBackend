using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Commands;

public record DeleteRefrigeratorItemCommand(int RefrigeratorId, int ItemId) : IRequest;

public class DeleteRefrigeratorItemCommandHandler : IRequestHandler<DeleteRefrigeratorItemCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteRefrigeratorItemCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteRefrigeratorItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _db.RefrigeratorItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.RefrigeratorId == request.RefrigeratorId, cancellationToken)
            ?? throw new NotFoundException(nameof(RefrigeratorItem), request.ItemId);

        _db.RefrigeratorItems.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
