using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.FarmerTransactions.Commands;

public record DeleteFarmerPaymentCommand(int FarmerTransactionId, int PaymentId) : IRequest;

public class DeleteFarmerPaymentCommandHandler : IRequestHandler<DeleteFarmerPaymentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteFarmerPaymentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteFarmerPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.FarmerPayments
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId && p.FarmerTransactionId == request.FarmerTransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(FarmerPayment), request.PaymentId);

        if (!_currentUser.IsAdmin && payment.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        var tx = await _context.FarmerTransactions
            .FirstOrDefaultAsync(t => t.Id == payment.FarmerTransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(FarmerTransaction), payment.FarmerTransactionId);
        tx.EnsureEditable();

        _context.FarmerPayments.Remove(payment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
