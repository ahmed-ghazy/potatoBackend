using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.TraderTransactions.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.TraderTransactions.Commands;

public record TraderTransactionItemInput(
    int? Id,
    DateTime Date,
    string? Description,
    decimal Weight,
    decimal Price,
    bool HasTare = true,
    bool HasCommission = true,
    decimal CommissionPerTon = 0m,
    int SackCount = 0,
    int? RefrigeratorId = null);

public record TraderPaymentInput(DateTime Date, decimal Amount, string? Notes);

public record CreateTraderTransactionCommand(
    int TraderId,
    string? Notes,
    IReadOnlyList<TraderTransactionItemInput> Items,
    IReadOnlyList<TraderPaymentInput>? Payments = null) : IRequest<TraderTransactionDto>;

public class CreateTraderTransactionCommandValidator : AbstractValidator<CreateTraderTransactionCommand>
{
    public CreateTraderTransactionCommandValidator()
    {
        RuleFor(x => x.TraderId).GreaterThan(0).WithMessage("التاجر مطلوب");
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.Items).NotEmpty().WithMessage("لا يمكن إنشاء فاتورة بدون عناصر");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Date).NotEmpty().WithMessage("تاريخ العنصر مطلوب");
            item.RuleFor(i => i.Description).MaximumLength(200);
            item.RuleFor(i => i.Weight).GreaterThan(0).WithMessage("وزن العنصر يجب أن يكون أكبر من صفر");
            item.RuleFor(i => i.Price).GreaterThanOrEqualTo(0);
            item.RuleFor(i => i.CommissionPerTon).GreaterThanOrEqualTo(0);
            item.RuleFor(i => i.SackCount).GreaterThanOrEqualTo(0).WithMessage("العدد لا يمكن أن يكون سالباً");
        });
        When(x => x.Payments is not null, () =>
        {
            RuleForEach(x => x.Payments!).ChildRules(payment =>
            {
                payment.RuleFor(p => p.Amount).GreaterThan(0).WithMessage("مبلغ الدفعة يجب أن يكون أكبر من صفر");
                payment.RuleFor(p => p.Notes).MaximumLength(500);
            });
        });
    }
}

public class CreateTraderTransactionCommandHandler
    : IRequestHandler<CreateTraderTransactionCommand, TraderTransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateTraderTransactionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TraderTransactionDto> Handle(CreateTraderTransactionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is not int tenantId)
            throw new ForbiddenAccessException("لا يوجد سياق شركة");

        var trader = await _context.Traders
            .FirstOrDefaultAsync(t => t.Id == request.TraderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Trader), request.TraderId);

        await EnsureRefrigeratorsExistAsync(request.Items, cancellationToken);

        var items = request.Items
            .Select(i => TraderTransactionItem.Create(i.Date, i.Description, i.SackCount, i.Weight, i.Price, i.HasTare, i.HasCommission, i.CommissionPerTon, i.RefrigeratorId))
            .ToList();

        var entity = TraderTransaction.Create(
            trader.Id,
            request.Notes,
            _currentUser.UserId,
            tenantId,
            items);

        _context.TraderTransactions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var payments = (request.Payments ?? Array.Empty<TraderPaymentInput>())
            .Select(p => TraderPayment.Create(entity.Id, p.Date, p.Amount, p.Notes, _currentUser.UserId, tenantId))
            .ToList();

        if (payments.Count > 0)
        {
            _context.TraderPayments.AddRange(payments);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var dto = TraderTransactionDto.From(entity, payments);
        dto.TraderName = trader.Name;
        dto.TraderPhoneNumber = trader.PhoneNumber;
        return dto;
    }

    private async Task EnsureRefrigeratorsExistAsync(
        IEnumerable<TraderTransactionItemInput> items, CancellationToken cancellationToken)
    {
        var refrigeratorIds = items
            .Where(i => i.RefrigeratorId is > 0)
            .Select(i => i.RefrigeratorId!.Value)
            .Distinct()
            .ToList();
        if (refrigeratorIds.Count == 0) return;

        var found = await _context.Refrigerators
            .Where(r => refrigeratorIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var missing = refrigeratorIds.Except(found).ToList();
        if (missing.Count > 0)
            throw new NotFoundException(nameof(Refrigerator), missing[0]);
    }
}
