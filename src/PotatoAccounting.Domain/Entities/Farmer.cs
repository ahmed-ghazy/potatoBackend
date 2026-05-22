using System.Text.RegularExpressions;
using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Domain.Entities;

public class Farmer : AuditableEntity, ITenantEntity
{
    private static readonly Regex EgyptPhoneRegex = new(@"^01[0125][0-9]{8}$", RegexOptions.Compiled);

    public string Name { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public int TenantId { get; private set; }

    private Farmer() { }

    public static Farmer Create(string name, string? phoneNumber, string? createdBy, int tenantId)
    {
        Guard(name, phoneNumber);

        return new Farmer
        {
            Name = name.Trim(),
            PhoneNumber = Normalize(phoneNumber),
            CreatedBy = createdBy,
            TenantId = tenantId,
        };
    }

    public void Update(string name, string? phoneNumber)
    {
        Guard(name, phoneNumber);
        Name = name.Trim();
        PhoneNumber = Normalize(phoneNumber);
    }

    public void SetTenant(int tenantId) => TenantId = tenantId;

    private static string? Normalize(string? phoneNumber)
        => string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();

    private static void Guard(string name, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("اسم الفلاح مطلوب");
        if (name.Trim().Length > 200)
            throw new DomainException("اسم الفلاح طويل جداً");

        var phone = Normalize(phoneNumber);
        if (phone is not null && !EgyptPhoneRegex.IsMatch(phone))
            throw new DomainException("رقم التليفون يجب أن يكون رقم مصرى صحيح (مثال: 01012345678)");
    }
}
