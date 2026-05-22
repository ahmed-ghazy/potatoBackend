using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Domain.Entities;

public class RefrigeratorItem : AuditableEntity, ITenantEntity
{
    public int RefrigeratorId { get; private set; }
    public DateTime Date { get; private set; }
    public int SackCount { get; private set; }
    public decimal Weight { get; private set; }
    public string? Description { get; private set; }
    public string? UserId { get; private set; }
    public int TenantId { get; private set; }

    private RefrigeratorItem() { }

    public static RefrigeratorItem Create(
        int refrigeratorId,
        DateTime date,
        int sackCount,
        decimal weight,
        string? description,
        string? userId,
        int tenantId)
    {
        Guard(sackCount, weight, description);

        return new RefrigeratorItem
        {
            RefrigeratorId = refrigeratorId,
            Date = date,
            SackCount = sackCount,
            Weight = weight,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            UserId = userId,
            CreatedBy = userId,
            TenantId = tenantId,
        };
    }

    public void Update(DateTime date, int sackCount, decimal weight, string? description)
    {
        Guard(sackCount, weight, description);
        Date = date;
        SackCount = sackCount;
        Weight = weight;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void SetTenant(int tenantId) => TenantId = tenantId;

    private static void Guard(int sackCount, decimal weight, string? description)
    {
        if (sackCount <= 0)
            throw new DomainException("عدد الشكاير يجب أن يكون أكبر من صفر");
        if (weight <= 0)
            throw new DomainException("الوزن يجب أن يكون أكبر من صفر");
        if (description is not null && description.Length > 200)
            throw new DomainException("وصف العنصر طويل جداً");
    }
}
