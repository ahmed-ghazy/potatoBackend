using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.FarmerTransactions.Commands;

public record DeleteFarmerTransactionItemImageCommand(int TransactionId, int ItemId) : IRequest;

public class DeleteFarmerTransactionItemImageCommandHandler : IRequestHandler<DeleteFarmerTransactionItemImageCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorage _storage;

    public DeleteFarmerTransactionItemImageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorage storage)
    {
        _context = context;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task Handle(DeleteFarmerTransactionItemImageCommand request, CancellationToken cancellationToken)
    {
        var tx = await _context.FarmerTransactions
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(FarmerTransaction), request.TransactionId);

        if (!_currentUser.IsAdmin && tx.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        tx.EnsureEditable();

        var item = tx.FindItem(request.ItemId)
            ?? throw new NotFoundException(nameof(FarmerTransactionItem), request.ItemId);

        var existing = item.ImagePath;
        if (string.IsNullOrEmpty(existing)) return;

        item.RemoveImage();
        await _context.SaveChangesAsync(cancellationToken);
        _storage.Delete(existing);
    }
}
