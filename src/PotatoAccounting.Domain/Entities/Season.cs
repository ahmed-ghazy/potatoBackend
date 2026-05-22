using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Domain.Entities;

public class Season : AuditableEntity, ITenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public bool IsDefault { get; private set; }
    public int TenantId { get; private set; }

    private Season() { }

    public static Season Create(string name, string? notes, string? createdBy, int tenantId, bool isDefault = false)
    {
        Guard(name, notes);

        return new Season
        {
            Name = name.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            IsDefault = isDefault,
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

    public void MarkAsDefault() => IsDefault = true;
    public void ClearDefault() => IsDefault = false;

    public void SetTenant(int tenantId) => TenantId = tenantId;

    private static void Guard(string name, string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("اسم الموسم مطلوب");
        if (name.Trim().Length > 200)
            throw new DomainException("اسم الموسم طويل جداً");
        if (notes is not null && notes.Length > 1000)
            throw new DomainException("الملاحظات طويلة جداً");
    }
}
