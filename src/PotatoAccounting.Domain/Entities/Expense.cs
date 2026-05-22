using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Domain.Entities;

public class Expense : AuditableEntity, ITenantEntity, ISeasonAwareEntity
{
    public DateTime Date { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? UserId { get; private set; }
    public int TenantId { get; private set; }
    public int SeasonId { get; private set; }

    private Expense() { }

    public static Expense Create(DateTime date, decimal amount, string description, string? userId, int tenantId)
    {
        Guard(amount, description);

        return new Expense
        {
            Date = date,
            Amount = amount,
            Description = description.Trim(),
            UserId = userId,
            CreatedBy = userId,
            TenantId = tenantId,
        };
    }

    public void Update(DateTime date, decimal amount, string description)
    {
        Guard(amount, description);

        Date = date;
        Amount = amount;
        Description = description.Trim();
    }

    public void SetTenant(int tenantId) => TenantId = tenantId;
    public void SetSeason(int seasonId) => SeasonId = seasonId;

    private static void Guard(decimal amount, string description)
    {
        if (amount <= 0)
            throw new DomainException("المبلغ يجب أن يكون أكبر من صفر");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("وصف المصروف مطلوب");
    }
}
