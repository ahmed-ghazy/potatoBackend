using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.TraderTransactions.Commands;

public record DeleteTraderTransactionCommand(int Id) : IRequest;

public class DeleteTraderTransactionCommandHandler : IRequestHandler<DeleteTraderTransactionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorage _storage;

    public DeleteTraderTransactionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorage storage)
    {
        _context = context;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task Handle(DeleteTraderTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TraderTransactions
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(TraderTransaction), request.Id);

        if (!_currentUser.IsAdmin && entity.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        entity.EnsureEditable();

        var imagePaths = entity.Items
            .Where(i => !string.IsNullOrEmpty(i.ImagePath))
            .Select(i => i.ImagePath!)
            .ToList();

        _context.TraderTransactions.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var path in imagePaths)
            _storage.Delete(path);
    }
}
