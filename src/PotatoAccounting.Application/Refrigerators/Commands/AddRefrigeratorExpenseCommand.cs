using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Refrigerators.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Commands;

public record AddRefrigeratorExpenseCommand(
    int RefrigeratorId,
    DateTime Date,
    decimal Amount,
    string Description,
    string? Notes) : IRequest<RefrigeratorExpenseDto>;

public class AddRefrigeratorExpenseCommandValidator : AbstractValidator<AddRefrigeratorExpenseCommand>
{
    public AddRefrigeratorExpenseCommandValidator()
    {
        RuleFor(x => x.RefrigeratorId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("المبلغ يجب أن يكون أكبر من صفر");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class AddRefrigeratorExpenseCommandHandler : IRequestHandler<AddRefrigeratorExpenseCommand, RefrigeratorExpenseDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AddRefrigeratorExpenseCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RefrigeratorExpenseDto> Handle(AddRefrigeratorExpenseCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is not int tenantId)
            throw new ForbiddenAccessException("لا يوجد سياق شركة");

        var r = await _db.Refrigerators.FirstOrDefaultAsync(x => x.Id == request.RefrigeratorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Refrigerator), request.RefrigeratorId);

        var expense = RefrigeratorExpense.Create(r.Id, request.Date, request.Amount, request.Description, request.Notes, _currentUser.UserId, tenantId);
        _db.RefrigeratorExpenses.Add(expense);
        await _db.SaveChangesAsync(cancellationToken);

        return RefrigeratorExpenseDto.From(expense);
    }
}
