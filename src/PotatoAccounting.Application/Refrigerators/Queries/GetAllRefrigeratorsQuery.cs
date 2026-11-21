using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Refrigerators.Dtos;

namespace PotatoAccounting.Application.Refrigerators.Queries;

public record GetAllRefrigeratorsQuery(string? Search = null) : IRequest<IReadOnlyList<RefrigeratorDto>>;

public class GetAllRefrigeratorsQueryHandler : IRequestHandler<GetAllRefrigeratorsQuery, IReadOnlyList<RefrigeratorDto>>
{
    private readonly IApplicationDbContext _db;

    public GetAllRefrigeratorsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<RefrigeratorDto>> Handle(GetAllRefrigeratorsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Refrigerators.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(r => EF.Functions.Like(r.Name, $"%{s}%"));
        }

        var refrigerators = await query.OrderBy(r => r.Name).ToListAsync(cancellationToken);
        var ids = refrigerators.Select(r => r.Id).ToList();

        var items = await _db.RefrigeratorItems.AsNoTracking()
            .Where(i => ids.Contains(i.RefrigeratorId)).ToListAsync(cancellationToken);
        var expenses = await _db.RefrigeratorExpenses.AsNoTracking()
            .Where(e => ids.Contains(e.RefrigeratorId)).ToListAsync(cancellationToken);

        // Sacks/weight taken out through trader invoices. Joining TraderTransactions keeps the
        // tenant + season query filter applied (TraderTransactionItem carries no filter of its own).
        var sold = await (from i in _db.TraderTransactionItems.AsNoTracking()
                          join t in _db.TraderTransactions.AsNoTracking() on i.TraderTransactionId equals t.Id
                          where i.RefrigeratorId != null && ids.Contains(i.RefrigeratorId.Value)
                          group i by i.RefrigeratorId!.Value into g
                          select new
                          {
                              RefrigeratorId = g.Key,
                              SackCount = g.Sum(x => x.SackCount),
                              Weight = g.Sum(x => x.Weight),
                          })
                          .ToListAsync(cancellationToken);

        var soldSacksByRid = sold.ToDictionary(x => x.RefrigeratorId, x => x.SackCount);
        var soldWeightByRid = sold.ToDictionary(x => x.RefrigeratorId, x => x.Weight);

        var itemsByRid = items.GroupBy(i => i.RefrigeratorId).ToDictionary(g => g.Key, g => g.ToList());
        var expensesByRid = expenses.GroupBy(e => e.RefrigeratorId).ToDictionary(g => g.Key, g => g.ToList());

        return refrigerators.Select(r => RefrigeratorDto.From(
            r,
            itemsByRid.TryGetValue(r.Id, out var iList) ? iList : null,
            expensesByRid.TryGetValue(r.Id, out var eList) ? eList : null,
            soldSacksByRid.GetValueOrDefault(r.Id),
            soldWeightByRid.GetValueOrDefault(r.Id)
        )).ToList();
    }
}
