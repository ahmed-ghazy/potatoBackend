using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Refrigerators.Dtos;

namespace PotatoAccounting.Application.Refrigerators.Queries;

public record GetRefrigeratorsSummaryQuery : IRequest<RefrigeratorsSummaryDto>;

public class GetRefrigeratorsSummaryQueryHandler : IRequestHandler<GetRefrigeratorsSummaryQuery, RefrigeratorsSummaryDto>
{
    private readonly IApplicationDbContext _db;

    public GetRefrigeratorsSummaryQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<RefrigeratorsSummaryDto> Handle(GetRefrigeratorsSummaryQuery request, CancellationToken cancellationToken)
    {
        var count = await _db.Refrigerators.CountAsync(cancellationToken);
        var totalSacks = await _db.RefrigeratorItems.SumAsync(i => (int?)i.SackCount, cancellationToken) ?? 0;
        var totalWeight = await _db.RefrigeratorItems.SumAsync(i => (decimal?)i.Weight, cancellationToken) ?? 0m;
        var totalExpenses = await _db.RefrigeratorExpenses.SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;

        // Only quantities attributed to a refrigerator count as "sold out of a refrigerator";
        // the join keeps the tenant + season filter on TraderTransactions in play.
        var soldQuery = from i in _db.TraderTransactionItems.AsNoTracking()
                        join t in _db.TraderTransactions.AsNoTracking() on i.TraderTransactionId equals t.Id
                        where i.RefrigeratorId != null
                        select i;

        var soldSacks = await soldQuery.SumAsync(i => (int?)i.SackCount, cancellationToken) ?? 0;
        var soldWeight = await soldQuery.SumAsync(i => (decimal?)i.Weight, cancellationToken) ?? 0m;

        return new RefrigeratorsSummaryDto
        {
            Count = count,
            TotalSackCount = totalSacks,
            TotalWeight = Math.Round(totalWeight, 2),
            TotalExpenses = Math.Round(totalExpenses, 2),
            TotalSoldSackCount = soldSacks,
            TotalRemainingSackCount = totalSacks - soldSacks,
            TotalSoldWeight = Math.Round(soldWeight, 2),
            TotalRemainingWeight = Math.Round(totalWeight - soldWeight, 2),
        };
    }
}
