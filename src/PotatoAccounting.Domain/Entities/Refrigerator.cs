using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Domain.Entities;

public class Refrigerator : AuditableEntity, ITenantEntity, ISeasonAwareEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    /// <summary>Path to the storage receipt image (وصل الاستلام) attached to the refrigerator.</summary>
    public string? ReceiptImagePath { get; private set; }
    public int TenantId { get; private set; }
    public int SeasonId { get; private set; }

    private Refrigerator() { }

    public static Refrigerator Create(string name, string? notes, string? createdBy, int tenantId)
    {
        Guard(name, notes);

        return new Refrigerator
        {
            Name = name.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedBy = createdBy,
            TenantId = tenantId,
        };
    }

    public void Update(string name, string? notes)
    {
        Guard(name, notes);
        Name = name.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void AttachReceiptImage(string? imagePath) =>
        ReceiptImagePath = string.IsNullOrWhiteSpace(imagePath) ? null : imagePath;

    public void RemoveReceiptImage() => ReceiptImagePath = null;

    public void SetTenant(int tenantId) => TenantId = tenantId;
    public void SetSeason(int seasonId) => SeasonId = seasonId;

    private static void Guard(string name, string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("اسم التلاجة مطلوب");
        if (name.Trim().Length > 200)
            throw new DomainException("اسم التلاجة طويل جداً");
        if (notes is not null && notes.Length > 1000)
            throw new DomainException("الملاحظات طويلة جداً");
    }
}
