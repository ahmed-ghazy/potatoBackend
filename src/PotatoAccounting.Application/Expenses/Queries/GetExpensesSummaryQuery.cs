using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Expenses.Dtos;

namespace PotatoAccounting.Application.Expenses.Queries;

public record GetExpensesSummaryQuery : IRequest<ExpensesSummaryDto>;

public class GetExpensesSummaryQueryHandler : IRequestHandler<GetExpensesSummaryQuery, ExpensesSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetExpensesSummaryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ExpensesSummaryDto> Handle(GetExpensesSummaryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Expenses.AsNoTracking();
        if (!_currentUser.IsAdmin)
            query = query.Where(e => e.UserId == _currentUser.UserId);

        return new ExpensesSummaryDto
        {
            Total = await query.SumAsync(e => e.Amount, cancellationToken),
            Count = await query.CountAsync(cancellationToken),
        };
    }
}
