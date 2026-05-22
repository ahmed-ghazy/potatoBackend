using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Refrigerators.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Commands;

public record UpdateRefrigeratorExpenseCommand(
    int RefrigeratorId,
    int ExpenseId,
    DateTime Date,
    decimal Amount,
    string Description,
    string? Notes) : IRequest<RefrigeratorExpenseDto>;

public class UpdateRefrigeratorExpenseCommandValidator : AbstractValidator<UpdateRefrigeratorExpenseCommand>
{
    public UpdateRefrigeratorExpenseCommandValidator()
    {
        RuleFor(x => x.RefrigeratorId).GreaterThan(0);
        RuleFor(x => x.ExpenseId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class UpdateRefrigeratorExpenseCommandHandler : IRequestHandler<UpdateRefrigeratorExpenseCommand, RefrigeratorExpenseDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateRefrigeratorExpenseCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<RefrigeratorExpenseDto> Handle(UpdateRefrigeratorExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _db.RefrigeratorExpenses
            .FirstOrDefaultAsync(e => e.Id == request.ExpenseId && e.RefrigeratorId == request.RefrigeratorId, cancellationToken)
            ?? throw new NotFoundException(nameof(RefrigeratorExpense), request.ExpenseId);

        expense.Update(request.Date, request.Amount, request.Description, request.Notes);
        await _db.SaveChangesAsync(cancellationToken);

        return RefrigeratorExpenseDto.From(expense);
    }
}
