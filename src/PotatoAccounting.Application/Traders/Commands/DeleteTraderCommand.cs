using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Traders.Commands;

public record DeleteTraderCommand(int Id) : IRequest;

public class DeleteTraderCommandHandler : IRequestHandler<DeleteTraderCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteTraderCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteTraderCommand request, CancellationToken cancellationToken)
    {
        var trader = await _db.Traders.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Trader), request.Id);

        var hasTransactions = await _db.TraderTransactions.AnyAsync(t => t.TraderId == request.Id, cancellationToken);
        if (hasTransactions)
            throw new Common.Exceptions.ValidationException(new[] { new ValidationFailure("id", "لا يمكن حذف التاجر لوجود فواتير مرتبطة به") });

        _db.Traders.Remove(trader);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
