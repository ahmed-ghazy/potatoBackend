using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Commands;

public record DeleteRefrigeratorItemImageCommand(int RefrigeratorId, int ItemId) : IRequest;

public class DeleteRefrigeratorItemImageCommandHandler : IRequestHandler<DeleteRefrigeratorItemImageCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorage _storage;

    public DeleteRefrigeratorItemImageCommandHandler(IApplicationDbContext db, IFileStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task Handle(DeleteRefrigeratorItemImageCommand request, CancellationToken cancellationToken)
    {
        var item = await _db.RefrigeratorItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.RefrigeratorId == request.RefrigeratorId, cancellationToken)
            ?? throw new NotFoundException(nameof(RefrigeratorItem), request.ItemId);

        var existing = item.ImagePath;
        if (string.IsNullOrEmpty(existing)) return;

        item.RemoveImage();
        await _db.SaveChangesAsync(cancellationToken);
        _storage.Delete(existing);
    }
}
