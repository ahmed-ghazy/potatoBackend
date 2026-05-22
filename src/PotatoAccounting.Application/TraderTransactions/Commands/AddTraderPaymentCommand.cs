using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.TraderTransactions.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.TraderTransactions.Commands;

public record AddTraderPaymentCommand(
    int TraderTransactionId,
    DateTime Date,
    decimal Amount,
    string? Notes) : IRequest<TraderPaymentDto>;

public class AddTraderPaymentCommandValidator : AbstractValidator<AddTraderPaymentCommand>
{
    public AddTraderPaymentCommandValidator()
    {
        RuleFor(x => x.TraderTransactionId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("مبلغ الدفعة يجب أن يكون أكبر من صفر");
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class AddTraderPaymentCommandHandler : IRequestHandler<AddTraderPaymentCommand, TraderPaymentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddTraderPaymentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TraderPaymentDto> Handle(AddTraderPaymentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is not int tenantId)
            throw new ForbiddenAccessException("لا يوجد سياق شركة");

        var tx = await _context.TraderTransactions
            .FirstOrDefaultAsync(t => t.Id == request.TraderTransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(TraderTransaction), request.TraderTransactionId);

        if (!_currentUser.IsAdmin && tx.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        tx.EnsureEditable();

        var payment = TraderPayment.Create(
            tx.Id,
            request.Date,
            request.Amount,
            request.Notes,
            _currentUser.UserId,
            tenantId);

        _context.TraderPayments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return TraderPaymentDto.From(payment);
    }
}
