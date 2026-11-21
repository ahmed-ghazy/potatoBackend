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
    /// <summary>
    /// Free-text owner label used to group items per "صاحب" inside one refrigerator
    /// (e.g. "محمد و محمود"). Frontend offers autocomplete from existing values.
    /// </summary>
    public string? OwnerName { get; private set; }
    public string? ImagePath { get; private set; }
    public string? UserId { get; private set; }
    public int TenantId { get; private set; }

    private RefrigeratorItem() { }

    public static RefrigeratorItem Create(
        int refrigeratorId,
        DateTime date,
        int sackCount,
        decimal weight,
        string? description,
        string? ownerName,
        string? userId,
        int tenantId)
    {
        Guard(sackCount, weight, description, ownerName);

        return new RefrigeratorItem
        {
            RefrigeratorId = refrigeratorId,
            Date = date,
            SackCount = sackCount,
            Weight = weight,
            Description = NormalizeText(description),
            OwnerName = NormalizeText(ownerName),
            UserId = userId,
            CreatedBy = userId,
            TenantId = tenantId,
        };
    }

    public void Update(DateTime date, int sackCount, decimal weight, string? description, string? ownerName)
    {
        Guard(sackCount, weight, description, ownerName);
        Date = date;
        SackCount = sackCount;
        Weight = weight;
        Description = NormalizeText(description);
        OwnerName = NormalizeText(ownerName);
    }

    public void AttachImage(string? imagePath) =>
        ImagePath = string.IsNullOrWhiteSpace(imagePath) ? null : imagePath;

    public void RemoveImage() => ImagePath = null;

    public void SetTenant(int tenantId) => TenantId = tenantId;

    private static string? NormalizeText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Guard(int sackCount, decimal weight, string? description, string? ownerName)
    {
        if (sackCount <= 0)
            throw new DomainException("عدد الشكاير يجب أن يكون أكبر من صفر");
        if (weight <= 0)
            throw new DomainException("الوزن يجب أن يكون أكبر من صفر");
        if (description is not null && description.Length > 200)
            throw new DomainException("وصف العنصر طويل جداً");
        if (ownerName is not null && ownerName.Length > 200)
            throw new DomainException("اسم صاحب العنصر طويل جداً");
    }
}
