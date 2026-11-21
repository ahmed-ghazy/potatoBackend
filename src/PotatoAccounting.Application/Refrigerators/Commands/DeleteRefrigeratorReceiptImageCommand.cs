using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Commands;

public record DeleteRefrigeratorReceiptImageCommand(int RefrigeratorId) : IRequest;

public class DeleteRefrigeratorReceiptImageCommandHandler : IRequestHandler<DeleteRefrigeratorReceiptImageCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorage _storage;

    public DeleteRefrigeratorReceiptImageCommandHandler(IApplicationDbContext db, IFileStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task Handle(DeleteRefrigeratorReceiptImageCommand request, CancellationToken cancellationToken)
    {
        var r = await _db.Refrigerators
            .FirstOrDefaultAsync(x => x.Id == request.RefrigeratorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Refrigerator), request.RefrigeratorId);

        var existing = r.ReceiptImagePath;
        if (string.IsNullOrEmpty(existing)) return;

        r.RemoveReceiptImage();
        await _db.SaveChangesAsync(cancellationToken);
        _storage.Delete(existing);
    }
}
