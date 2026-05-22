using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.FarmerTransactions.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.FarmerTransactions.Commands;

public record AddFarmerPaymentCommand(
    int FarmerTransactionId,
    DateTime Date,
    decimal Amount,
    string? Notes) : IRequest<FarmerPaymentDto>;

public class AddFarmerPaymentCommandValidator : AbstractValidator<AddFarmerPaymentCommand>
{
    public AddFarmerPaymentCommandValidator()
    {
        RuleFor(x => x.FarmerTransactionId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("مبلغ الدفعة يجب أن يكون أكبر من صفر");
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class AddFarmerPaymentCommandHandler : IRequestHandler<AddFarmerPaymentCommand, FarmerPaymentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddFarmerPaymentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<FarmerPaymentDto> Handle(AddFarmerPaymentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is not int tenantId)
            throw new ForbiddenAccessException("لا يوجد سياق شركة");

        var tx = await _context.FarmerTransactions
            .FirstOrDefaultAsync(t => t.Id == request.FarmerTransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(FarmerTransaction), request.FarmerTransactionId);

        if (!_currentUser.IsAdmin && tx.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        tx.EnsureEditable();

        var payment = FarmerPayment.Create(
            tx.Id,
            request.Date,
            request.Amount,
            request.Notes,
            _currentUser.UserId,
            tenantId);

        _context.FarmerPayments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return FarmerPaymentDto.From(payment);
    }
}
