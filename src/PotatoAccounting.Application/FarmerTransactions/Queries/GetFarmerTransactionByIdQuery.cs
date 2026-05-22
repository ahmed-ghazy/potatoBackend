using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.FarmerTransactions.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.FarmerTransactions.Queries;

public record GetFarmerTransactionByIdQuery(int Id) : IRequest<FarmerTransactionDto>;

public class GetFarmerTransactionByIdQueryHandler
    : IRequestHandler<GetFarmerTransactionByIdQuery, FarmerTransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetFarmerTransactionByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<FarmerTransactionDto> Handle(GetFarmerTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.FarmerTransactions
            .AsNoTracking()
            .Include(t => t.Farmer)
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(FarmerTransaction), request.Id);

        if (!_currentUser.IsAdmin && entity.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        var payments = await _context.FarmerPayments
            .AsNoTracking()
            .Where(p => p.FarmerTransactionId == entity.Id)
            .ToListAsync(cancellationToken);

        return FarmerTransactionDto.From(entity, payments);
    }
}
