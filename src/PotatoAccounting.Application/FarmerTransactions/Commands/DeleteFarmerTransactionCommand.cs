using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.FarmerTransactions.Commands;

public record DeleteFarmerTransactionCommand(int Id) : IRequest;

public class DeleteFarmerTransactionCommandHandler : IRequestHandler<DeleteFarmerTransactionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorage _storage;

    public DeleteFarmerTransactionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorage storage)
    {
        _context = context;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task Handle(DeleteFarmerTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FarmerTransactions
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FarmerTransaction), request.Id);

        if (!_currentUser.IsAdmin && entity.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        entity.EnsureEditable();

        var imagePaths = entity.Items
            .Where(i => !string.IsNullOrEmpty(i.ImagePath))
            .Select(i => i.ImagePath!)
            .ToList();

        _context.FarmerTransactions.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var path in imagePaths)
            _storage.Delete(path);
    }
}
