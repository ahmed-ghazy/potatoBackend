using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Domain.Entities;

public class Tenant : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private Tenant() { }

    public static Tenant Create(string name, string? createdBy)
    {
        GuardName(name);
        return new Tenant
        {
            Name = name.Trim(),
            IsActive = true,
            CreatedBy = createdBy,
        };
    }

    public void Rename(string name)
    {
        GuardName(name);
        Name = name.Trim();
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    private static void GuardName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("اسم الشركة مطلوب");
        if (name.Trim().Length > 200)
            throw new DomainException("اسم الشركة طويل جداً");
    }
}
