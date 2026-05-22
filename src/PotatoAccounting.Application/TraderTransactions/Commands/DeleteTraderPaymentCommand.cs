using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.TraderTransactions.Commands;

public record DeleteTraderPaymentCommand(int TraderTransactionId, int PaymentId) : IRequest;

public class DeleteTraderPaymentCommandHandler : IRequestHandler<DeleteTraderPaymentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteTraderPaymentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteTraderPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.TraderPayments
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId && p.TraderTransactionId == request.TraderTransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(TraderPayment), request.PaymentId);

        if (!_currentUser.IsAdmin && payment.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        var tx = await _context.TraderTransactions
            .FirstOrDefaultAsync(t => t.Id == payment.TraderTransactionId, cancellationToken)
            ?? throw new NotFoundException(nameof(TraderTransaction), payment.TraderTransactionId);
        tx.EnsureEditable();

        _context.TraderPayments.Remove(payment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
