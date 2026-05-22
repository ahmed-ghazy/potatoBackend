using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Common.Models;
using PotatoAccounting.Application.Expenses.Dtos;

namespace PotatoAccounting.Application.Expenses.Queries;

public record GetAllExpensesQuery(
    string? Search = null,
    DateTime? From = null,
    DateTime? To = null,
    int? Page = null,
    int? PageSize = null) : IRequest<PagedResult<ExpenseDto>>;

public class GetAllExpensesQueryHandler : IRequestHandler<GetAllExpensesQuery, PagedResult<ExpenseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAllExpensesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ExpenseDto>> Handle(GetAllExpensesQuery request, CancellationToken cancellationToken)
    {
        var (page, pageSize) = PagingDefaults.Normalize(request.Page, request.PageSize);

        var query = _context.Expenses.AsNoTracking();
        if (!_currentUser.IsAdmin)
            query = query.Where(e => e.UserId == _currentUser.UserId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            query = query.Where(e => EF.Functions.Like(e.Description, $"%{s}%"));
        }

        if (request.From.HasValue)
        {
            var from = request.From.Value.Date;
            query = query.Where(e => e.Date >= from);
        }
        if (request.To.HasValue)
        {
            var to = request.To.Value.Date.AddDays(1);
            query = query.Where(e => e.Date < to);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ExpenseDto>
        {
            Items = items.Select(ExpenseDto.From).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }
}
