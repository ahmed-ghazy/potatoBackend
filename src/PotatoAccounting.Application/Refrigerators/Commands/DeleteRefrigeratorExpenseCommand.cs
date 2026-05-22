using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Commands;

public record DeleteRefrigeratorExpenseCommand(int RefrigeratorId, int ExpenseId) : IRequest;

public class DeleteRefrigeratorExpenseCommandHandler : IRequestHandler<DeleteRefrigeratorExpenseCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteRefrigeratorExpenseCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteRefrigeratorExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _db.RefrigeratorExpenses
            .FirstOrDefaultAsync(e => e.Id == request.ExpenseId && e.RefrigeratorId == request.RefrigeratorId, cancellationToken)
            ?? throw new NotFoundException(nameof(RefrigeratorExpense), request.ExpenseId);

        _db.RefrigeratorExpenses.Remove(expense);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
