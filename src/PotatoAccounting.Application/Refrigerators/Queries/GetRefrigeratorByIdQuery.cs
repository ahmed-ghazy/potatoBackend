using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Refrigerators.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Queries;

public record GetRefrigeratorByIdQuery(int Id) : IRequest<RefrigeratorDto>;

public class GetRefrigeratorByIdQueryHandler : IRequestHandler<GetRefrigeratorByIdQuery, RefrigeratorDto>
{
    private readonly IApplicationDbContext _db;

    public GetRefrigeratorByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<RefrigeratorDto> Handle(GetRefrigeratorByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await _db.Refrigerators.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Refrigerator), request.Id);

        var items = await _db.RefrigeratorItems.AsNoTracking()
            .Where(i => i.RefrigeratorId == r.Id).ToListAsync(cancellationToken);
        var expenses = await _db.RefrigeratorExpenses.AsNoTracking()
            .Where(e => e.RefrigeratorId == r.Id).ToListAsync(cancellationToken);

        // Sacks/weight taken out through trader invoices — joined to TraderTransactions so the
        // tenant + season query filter applies.
        var sold = await (from i in _db.TraderTransactionItems.AsNoTracking()
                          join t in _db.TraderTransactions.AsNoTracking() on i.TraderTransactionId equals t.Id
                          where i.RefrigeratorId == r.Id
                          group i by i.RefrigeratorId into g
                          select new
                          {
                              SackCount = g.Sum(x => x.SackCount),
                              Weight = g.Sum(x => x.Weight),
                          })
                          .FirstOrDefaultAsync(cancellationToken);

        return RefrigeratorDto.From(r, items, expenses, sold?.SackCount ?? 0, sold?.Weight ?? 0m);
    }
}
