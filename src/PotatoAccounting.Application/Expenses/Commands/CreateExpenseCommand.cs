using FluentValidation;
using MediatR;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Expenses.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Expenses.Commands;

public record CreateExpenseCommand(DateTime Date, decimal Amount, string Description) : IRequest<ExpenseDto>;

public class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}

public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, ExpenseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateExpenseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ExpenseDto> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is not int tenantId)
            throw new Common.Exceptions.ForbiddenAccessException("لا يوجد سياق شركة");

        var entity = Expense.Create(request.Date, request.Amount, request.Description, _currentUser.UserId, tenantId);

        _context.Expenses.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ExpenseDto.From(entity);
    }
}
