using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Expenses.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Expenses.Queries;

public record GetExpenseByIdQuery(int Id) : IRequest<ExpenseDto>;

public class GetExpenseByIdQueryHandler : IRequestHandler<GetExpenseByIdQuery, ExpenseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetExpenseByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ExpenseDto> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Expenses
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Expense), request.Id);

        if (!_currentUser.IsAdmin && entity.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        return ExpenseDto.From(entity);
    }
}
