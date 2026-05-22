using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.FarmerTransactions.Dtos;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.FarmerTransactions.Commands;

public record FarmerTransactionItemInput(int? Id, string? Description, decimal Weight, decimal Price, bool HasTare = true);

public record FarmerPaymentInput(DateTime Date, decimal Amount, string? Notes);

public record CreateFarmerTransactionCommand(
    DateTime Date,
    int FarmerId,
    string? Notes,
    IReadOnlyList<FarmerTransactionItemInput> Items,
    IReadOnlyList<FarmerPaymentInput>? Payments = null) : IRequest<FarmerTransactionDto>;

public class CreateFarmerTransactionCommandValidator : AbstractValidator<CreateFarmerTransactionCommand>
{
    public CreateFarmerTransactionCommandValidator()
    {
        RuleFor(x => x.FarmerId).GreaterThan(0).WithMessage("الفلاح مطلوب");
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description).MaximumLength(200);
            item.RuleFor(i => i.Weight).GreaterThan(0).WithMessage("وزن العنصر يجب أن يكون أكبر من صفر");
            item.RuleFor(i => i.Price).GreaterThanOrEqualTo(0);
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

public class CreateFarmerTransactionCommandHandler
    : IRequestHandler<CreateFarmerTransactionCommand, FarmerTransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateFarmerTransactionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<FarmerTransactionDto> Handle(CreateFarmerTransactionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.TenantId is not int tenantId)
            throw new ForbiddenAccessException("لا يوجد سياق شركة");

        var farmer = await _context.Farmers
            .FirstOrDefaultAsync(f => f.Id == request.FarmerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Farmer), request.FarmerId);

        var items = (request.Items ?? Array.Empty<FarmerTransactionItemInput>())
            .Select(i => FarmerTransactionItem.Create(i.Description, i.Weight, i.Price, i.HasTare))
            .ToList();

        var entity = FarmerTransaction.Create(
            request.Date,
            farmer.Id,
            request.Notes,
            _currentUser.UserId,
            tenantId,
            items);

        _context.FarmerTransactions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var payments = (request.Payments ?? Array.Empty<FarmerPaymentInput>())
            .Select(p => FarmerPayment.Create(entity.Id, p.Date, p.Amount, p.Notes, _currentUser.UserId, tenantId))
            .ToList();

        if (payments.Count > 0)
        {
            _context.FarmerPayments.AddRange(payments);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var dto = FarmerTransactionDto.From(entity, payments);
        dto.FarmerName = farmer.Name;
        dto.FarmerPhoneNumber = farmer.PhoneNumber;
        return dto;
    }
}
