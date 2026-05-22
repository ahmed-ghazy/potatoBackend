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

        return new RefrigeratorsSummaryDto
        {
            Count = count,
            TotalSackCount = totalSacks,
            TotalWeight = Math.Round(totalWeight, 2),
            TotalExpenses = Math.Round(totalExpenses, 2),
        };
    }
}
