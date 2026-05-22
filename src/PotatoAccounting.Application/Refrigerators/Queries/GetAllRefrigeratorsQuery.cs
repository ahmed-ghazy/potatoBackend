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

        var itemsByRid = items.GroupBy(i => i.RefrigeratorId).ToDictionary(g => g.Key, g => g.ToList());
        var expensesByRid = expenses.GroupBy(e => e.RefrigeratorId).ToDictionary(g => g.Key, g => g.ToList());

        return refrigerators.Select(r => RefrigeratorDto.From(
            r,
            itemsByRid.TryGetValue(r.Id, out var iList) ? iList : null,
            expensesByRid.TryGetValue(r.Id, out var eList) ? eList : null
        )).ToList();
    }
}
