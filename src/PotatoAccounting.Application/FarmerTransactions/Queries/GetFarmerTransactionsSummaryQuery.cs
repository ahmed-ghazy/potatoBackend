using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.FarmerTransactions.Dtos;

namespace PotatoAccounting.Application.FarmerTransactions.Queries;

public record GetFarmerTransactionsSummaryQuery : IRequest<FarmerTransactionsSummaryDto>;

public class GetFarmerTransactionsSummaryQueryHandler
    : IRequestHandler<GetFarmerTransactionsSummaryQuery, FarmerTransactionsSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetFarmerTransactionsSummaryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<FarmerTransactionsSummaryDto> Handle(GetFarmerTransactionsSummaryQuery request, CancellationToken cancellationToken)
    {
        var txQuery = _context.FarmerTransactions
            .AsNoTracking()
            .Include(t => t.Items)
            .AsQueryable();

        var paymentsQuery = _context.FarmerPayments.AsNoTracking().AsQueryable();

        if (!_currentUser.IsAdmin)
        {
            txQuery = txQuery.Where(t => t.UserId == _currentUser.UserId);
            paymentsQuery = paymentsQuery.Where(p => p.UserId == _currentUser.UserId);
        }

        var transactions = await txQuery.ToListAsync(cancellationToken);
        var totalAmount = transactions.Sum(t => t.TotalAmount);
        var totalWeight = transactions.Sum(t => t.TotalWeight);
        var totalNetWeight = transactions.Sum(t => t.TotalNetWeight);
        var count = transactions.Count;

        var totalPaid = await paymentsQuery.SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
        totalPaid = Math.Round(totalPaid, 2);

        var remainingRounded = Math.Round(totalAmount - totalPaid, MidpointRounding.AwayFromZero);
        return new FarmerTransactionsSummaryDto
        {
            Total = Math.Round(totalAmount, 2),
            Paid = totalPaid,
            Remaining = remainingRounded > 0 ? remainingRounded : 0,
            Count = count,
            TotalWeight = Math.Round(totalWeight, 2),
            TotalNetWeight = Math.Round(totalNetWeight, 2),
        };
    }
}
