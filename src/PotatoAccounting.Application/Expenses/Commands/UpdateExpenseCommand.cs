using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Expenses.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Expenses.Commands;

public record UpdateExpenseCommand(int Id, DateTime Date, decimal Amount, string Description) : IRequest<ExpenseDto>;

public class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}

public class UpdateExpenseCommandHandler : IRequestHandler<UpdateExpenseCommand, ExpenseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateExpenseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ExpenseDto> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Expense), request.Id);

        if (!_currentUser.IsAdmin && entity.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        entity.Update(request.Date, request.Amount, request.Description);

        await _context.SaveChangesAsync(cancellationToken);

        return ExpenseDto.From(entity);
    }
}
