using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Common.Models;
using PotatoAccounting.Application.FarmerTransactions.Dtos;

namespace PotatoAccounting.Application.FarmerTransactions.Queries;

public record GetAllFarmerTransactionsQuery(
    string? Search = null,
    DateTime? From = null,
    DateTime? To = null,
    int? Page = null,
    int? PageSize = null) : IRequest<PagedResult<FarmerTransactionDto>>;

public class GetAllFarmerTransactionsQueryHandler
    : IRequestHandler<GetAllFarmerTransactionsQuery, PagedResult<FarmerTransactionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAllFarmerTransactionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<FarmerTransactionDto>> Handle(GetAllFarmerTransactionsQuery request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = PagingDefaults.Normalize(request.Page, request.PageSize);

        var query = _context.FarmerTransactions
            .AsNoTracking()
            .Include(t => t.Farmer)
            .Include(t => t.Items)
            .AsQueryable();

        if (!_currentUser.IsAdmin)
            query = query.Where(t => t.UserId == _currentUser.UserId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(t => t.Farmer != null && EF.Functions.Like(t.Farmer.Name, $"%{s}%"));
        }

        if (request.From.HasValue)
        {
            var from = request.From.Value.Date;
            query = query.Where(t => t.Date >= from);
        }
        if (request.To.HasValue)
        {
            var to = request.To.Value.Date.AddDays(1);
            query = query.Where(t => t.Date < to);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var ids = items.Select(i => i.Id).ToList();
        var payments = await _context.FarmerPayments
            .AsNoTracking()
            .Where(p => ids.Contains(p.FarmerTransactionId))
            .ToListAsync(cancellationToken);
        var paymentsByTx = payments.GroupBy(p => p.FarmerTransactionId).ToDictionary(g => g.Key, g => g.ToList());

        return new PagedResult<FarmerTransactionDto>
        {
            Items = items.Select(t => FarmerTransactionDto.From(
                t,
                paymentsByTx.TryGetValue(t.Id, out var list) ? list : null)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }
}
