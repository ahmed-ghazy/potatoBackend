using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.FarmerTransactions.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.FarmerTransactions.Commands;

public record ReopenFarmerTransactionCommand(int Id) : IRequest<FarmerTransactionDto>;

public class ReopenFarmerTransactionCommandHandler : IRequestHandler<ReopenFarmerTransactionCommand, FarmerTransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ReopenFarmerTransactionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<FarmerTransactionDto> Handle(ReopenFarmerTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FarmerTransactions
            .Include(t => t.Farmer)
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FarmerTransaction), request.Id);

        if (!_currentUser.IsAdmin && entity.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        entity.Reopen();
        await _context.SaveChangesAsync(cancellationToken);

        var payments = await _context.FarmerPayments
            .AsNoTracking()
            .Where(p => p.FarmerTransactionId == entity.Id)
            .ToListAsync(cancellationToken);

        return FarmerTransactionDto.From(entity, payments);
    }
}
