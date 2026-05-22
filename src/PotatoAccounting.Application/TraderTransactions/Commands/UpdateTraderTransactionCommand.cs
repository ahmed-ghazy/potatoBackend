using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.TraderTransactions.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.TraderTransactions.Commands;

public record UpdateTraderTransactionCommand(
    int Id,
    int TraderId,
    string? Notes,
    IReadOnlyList<TraderTransactionItemInput> Items) : IRequest<TraderTransactionDto>;

public class UpdateTraderTransactionCommandValidator : AbstractValidator<UpdateTraderTransactionCommand>
{
    public UpdateTraderTransactionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.TraderId).GreaterThan(0).WithMessage("التاجر مطلوب");
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.Items).NotEmpty().WithMessage("لا يمكن حفظ فاتورة بدون عناصر");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Date).NotEmpty().WithMessage("تاريخ العنصر مطلوب");
            item.RuleFor(i => i.Description).MaximumLength(200);
            item.RuleFor(i => i.Weight).GreaterThan(0).WithMessage("وزن العنصر يجب أن يكون أكبر من صفر");
            item.RuleFor(i => i.Price).GreaterThanOrEqualTo(0);
            item.RuleFor(i => i.CommissionPerTon).GreaterThanOrEqualTo(0);
        });
    }
}

public class UpdateTraderTransactionCommandHandler
    : IRequestHandler<UpdateTraderTransactionCommand, TraderTransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorage _storage;

    public UpdateTraderTransactionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorage storage)
    {
        _context = context;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<TraderTransactionDto> Handle(UpdateTraderTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TraderTransactions
            .Include(t => t.Trader)
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(TraderTransaction), request.Id);

        if (!_currentUser.IsAdmin && entity.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException();

        entity.EnsureEditable();

        if (entity.TraderId != request.TraderId)
        {
            var traderExists = await _context.Traders.AnyAsync(t => t.Id == request.TraderId, cancellationToken);
            if (!traderExists)
                throw new NotFoundException(nameof(Trader), request.TraderId);
        }

        entity.UpdateHeader(request.TraderId, request.Notes);

        // Diff items by Id
        var keepIds = request.Items.Where(i => i.Id is > 0).Select(i => i.Id!.Value).ToList();
        var removedItems = entity.RemoveItemsNotIn(keepIds);

        foreach (var input in request.Items)
        {
            if (input.Id is int id && id > 0)
            {
                var existing = entity.FindItem(id);
                if (existing is null)
                    throw new NotFoundException(nameof(TraderTransactionItem), id);
                existing.Update(input.Date, input.Description, input.Weight, input.Price, input.HasTare, input.HasCommission, input.CommissionPerTon);
            }
            else
            {
                entity.AddItem(TraderTransactionItem.Create(input.Date, input.Description, input.Weight, input.Price, input.HasTare, input.HasCommission, input.CommissionPerTon));
            }
        }

        entity.EnsureHasItems();
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var removed in removedItems)
        {
            if (!string.IsNullOrEmpty(removed.ImagePath))
                _storage.Delete(removed.ImagePath);
        }

        var trader = await _context.Traders.AsNoTracking().FirstAsync(t => t.Id == entity.TraderId, cancellationToken);
        var payments = await _context.TraderPayments
            .AsNoTracking()
            .Where(p => p.TraderTransactionId == entity.Id)
            .ToListAsync(cancellationToken);

        var dto = TraderTransactionDto.From(entity, payments);
        dto.TraderName = trader.Name;
        dto.TraderPhoneNumber = trader.PhoneNumber;
        return dto;
    }
}
