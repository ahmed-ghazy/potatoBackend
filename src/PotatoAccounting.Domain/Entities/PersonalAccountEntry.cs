using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Domain.Entities;

public class PersonalAccountEntry : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// Fee rate applied to Vodafone Cash received entries (1%).
    /// </summary>
    public const decimal VodafoneCashFeeRate = 0.01m;

    public int PersonalAccountId { get; private set; }
    public DateTime Date { get; private set; }
    public PersonalAccountEntryDirection Direction { get; private set; }
    /// <summary>
    /// The gross amount as entered. For Vodafone Cash received entries the
    /// effective amount in totals is <see cref="Amount"/> - <see cref="Fee"/>.
    /// </summary>
    public decimal Amount { get; private set; }
    /// <summary>
    /// Provider fee deducted from <see cref="Amount"/>. Currently non-zero only
    /// for Vodafone Cash received entries (1% of amount).
    /// </summary>
    public decimal Fee { get; private set; }
    public string? Description { get; private set; }
    /// <summary>
    /// How the money was transferred. Required for Received entries
    /// (cash / Vodafone Cash / bank transfer / InstaPay); null for Given.
    /// </summary>
    public PersonalAccountPaymentMethod? PaymentMethod { get; private set; }
    public string? UserId { get; private set; }
    public int TenantId { get; private set; }

    private PersonalAccountEntry() { }

    public static PersonalAccountEntry Create(
        int personalAccountId,
        DateTime date,
        PersonalAccountEntryDirection direction,
        decimal amount,
        string? description,
        PersonalAccountPaymentMethod? paymentMethod,
        string? userId,
        int tenantId)
    {
        Guard(direction, amount, description, paymentMethod);

        var effectiveMethod = direction == PersonalAccountEntryDirection.Received ? paymentMethod : null;
        return new PersonalAccountEntry
        {
            PersonalAccountId = personalAccountId,
            Date = date,
            Direction = direction,
            Amount = amount,
            Fee = ComputeFee(direction, effectiveMethod, amount),
            Description = NormalizeDescription(description),
            PaymentMethod = effectiveMethod,
            UserId = userId,
            CreatedBy = userId,
            TenantId = tenantId,
        };
    }

    public void Update(
        DateTime date,
        PersonalAccountEntryDirection direction,
        decimal amount,
        string? description,
        PersonalAccountPaymentMethod? paymentMethod)
    {
        Guard(direction, amount, description, paymentMethod);
        var effectiveMethod = direction == PersonalAccountEntryDirection.Received ? paymentMethod : null;
        Date = date;
        Direction = direction;
        Amount = amount;
        Fee = ComputeFee(direction, effectiveMethod, amount);
        Description = NormalizeDescription(description);
        PaymentMethod = effectiveMethod;
    }

    public void SetTenant(int tenantId) => TenantId = tenantId;

    private static decimal ComputeFee(
        PersonalAccountEntryDirection direction,
        PersonalAccountPaymentMethod? paymentMethod,
        decimal amount)
    {
        if (direction != PersonalAccountEntryDirection.Received) return 0m;
        if (paymentMethod != PersonalAccountPaymentMethod.VodafoneCash) return 0m;
        return Math.Round(amount * VodafoneCashFeeRate, 2, MidpointRounding.AwayFromZero);
    }

    private static string? NormalizeDescription(string? description)
        => string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static void Guard(
        PersonalAccountEntryDirection direction,
        decimal amount,
        string? description,
        PersonalAccountPaymentMethod? paymentMethod)
    {
        if (direction != PersonalAccountEntryDirection.Given && direction != PersonalAccountEntryDirection.Received)
            throw new DomainException("نوع الحركة غير صحيح");
        if (amount <= 0)
            throw new DomainException("المبلغ يجب أن يكون أكبر من صفر");
        if (description is not null && description.Length > 500)
            throw new DomainException("وصف الحركة طويل جداً");

        if (direction == PersonalAccountEntryDirection.Received)
        {
            if (paymentMethod is null)
                throw new DomainException("نوع الاستلام مطلوب");
            if (!Enum.IsDefined(typeof(PersonalAccountPaymentMethod), paymentMethod.Value))
                throw new DomainException("نوع الاستلام غير صحيح");
        }
    }
}
