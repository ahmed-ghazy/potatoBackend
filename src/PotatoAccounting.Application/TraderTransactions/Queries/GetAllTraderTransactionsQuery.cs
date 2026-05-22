using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Common.Models;
using PotatoAccounting.Application.TraderTransactions.Dtos;

namespace PotatoAccounting.Application.TraderTransactions.Queries;

public record GetAllTraderTransactionsQuery(
    string? Search = null,
    DateTime? From = null,
    DateTime? To = null,
    int? Page = null,
    int? PageSize = null) : IRequest<PagedResult<TraderTransactionDto>>;

public class GetAllTraderTransactionsQueryHandler
    : IRequestHandler<GetAllTraderTransactionsQuery, PagedResult<TraderTransactionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAllTraderTransactionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<TraderTransactionDto>> Handle(GetAllTraderTransactionsQuery request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = PagingDefaults.Normalize(request.Page, request.PageSize);

        var query = _context.TraderTransactions
            .AsNoTracking()
            .Include(t => t.Trader)
            .Include(t => t.Items)
            .AsQueryable();

        if (!_currentUser.IsAdmin)
            query = query.Where(t => t.UserId == _currentUser.UserId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(t => t.Trader != null && EF.Functions.Like(t.Trader.Name, $"%{s}%"));
        }

        // Filter on whether any item in the transaction is within the date range
        if (request.From.HasValue)
        {
            var from = request.From.Value.Date;
            query = query.Where(t => t.Items.Any(i => i.Date >= from));
        }
        if (request.To.HasValue)
        {
            var to = request.To.Value.Date.AddDays(1);
            query = query.Where(t => t.Items.Any(i => i.Date < to));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Sort by latest item date desc, then Id desc
        var items = await query
            .OrderByDescending(t => t.Items.Max(i => (DateTime?)i.Date) ?? DateTime.MinValue)
            .ThenByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var ids = items.Select(i => i.Id).ToList();
        var payments = await _context.TraderPayments
            .AsNoTracking()
            .Where(p => ids.Contains(p.TraderTransactionId))
            .ToListAsync(cancellationToken);
        var paymentsByTx = payments.GroupBy(p => p.TraderTransactionId).ToDictionary(g => g.Key, g => g.ToList());

        return new PagedResult<TraderTransactionDto>
        {
            Items = items.Select(t => TraderTransactionDto.From(
                t,
                paymentsByTx.TryGetValue(t.Id, out var list) ? list : null)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }
}
