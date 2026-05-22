using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.TraderTransactions.Dtos;

namespace PotatoAccounting.Application.TraderTransactions.Queries;

public record GetTraderTransactionsSummaryQuery : IRequest<TraderTransactionsSummaryDto>;

public class GetTraderTransactionsSummaryQueryHandler
    : IRequestHandler<GetTraderTransactionsSummaryQuery, TraderTransactionsSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTraderTransactionsSummaryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TraderTransactionsSummaryDto> Handle(GetTraderTransactionsSummaryQuery request, CancellationToken cancellationToken)
    {
        var txQuery = _context.TraderTransactions
            .AsNoTracking()
            .Include(t => t.Items)
            .AsQueryable();

        var paymentsQuery = _context.TraderPayments.AsNoTracking().AsQueryable();

        if (!_currentUser.IsAdmin)
        {
            txQuery = txQuery.Where(t => t.UserId == _currentUser.UserId);
            paymentsQuery = paymentsQuery.Where(p => p.UserId == _currentUser.UserId);
        }

        var transactions = await txQuery.ToListAsync(cancellationToken);
        var totalAmount = transactions.Sum(t => t.TotalAmount);
        var totalCommission = transactions.Sum(t => t.TotalCommission);
        var totalWeight = transactions.Sum(t => t.TotalWeight);
        var totalNetWeight = transactions.Sum(t => t.TotalNetWeight);
        var count = transactions.Count;

        var totalPaid = await paymentsQuery.SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
        totalPaid = Math.Round(totalPaid, 2);

        var remainingRounded = Math.Round(totalAmount - totalPaid, MidpointRounding.AwayFromZero);
        return new TraderTransactionsSummaryDto
        {
            Total = Math.Round(totalAmount, 2),
            Paid = totalPaid,
            Remaining = remainingRounded > 0 ? remainingRounded : 0,
            TotalCommission = Math.Round(totalCommission, 2),
            Count = count,
            TotalWeight = Math.Round(totalWeight, 2),
            TotalNetWeight = Math.Round(totalNetWeight, 2),
        };
    }
}
