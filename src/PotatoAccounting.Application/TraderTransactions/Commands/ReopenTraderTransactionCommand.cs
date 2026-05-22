using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.TraderTransactions.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.TraderTransactions.Commands;

public record ReopenTraderTransactionCommand(int Id) : IRequest<TraderTransactionDto>;

public class ReopenTraderTransactionCommandHandler : IRequestHandler<ReopenTraderTransactionCommand, TraderTransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ReopenTraderTransactionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TraderTransactionDto> Handle(ReopenTraderTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TraderTransactions
            .Include(t => t.Trader)
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(TraderTransaction), request.Id);

        if (!_currentUser.IsAdmin && entity.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        entity.Reopen();
        await _context.SaveChangesAsync(cancellationToken);

        var payments = await _context.TraderPayments
            .AsNoTracking()
            .Where(p => p.TraderTransactionId == entity.Id)
            .ToListAsync(cancellationToken);

        return TraderTransactionDto.From(entity, payments);
    }
}
