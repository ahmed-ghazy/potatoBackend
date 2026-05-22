using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Expenses.Commands;

public record DeleteExpenseCommand(int Id) : IRequest;

public class DeleteExpenseCommandHandler : IRequestHandler<DeleteExpenseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteExpenseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Expense), request.Id);

        if (!_currentUser.IsAdmin && entity.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        _context.Expenses.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
