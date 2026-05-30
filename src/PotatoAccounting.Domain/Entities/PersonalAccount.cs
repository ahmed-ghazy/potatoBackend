using System.Text.RegularExpressions;
using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Domain.Entities;

public class PersonalAccount : AuditableEntity, ITenantEntity
{
    private static readonly Regex EgyptPhoneRegex = new(@"^01[0125][0-9]{8}$", RegexOptions.Compiled);

    public string Name { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string? Notes { get; private set; }
    public int TenantId { get; private set; }

    private PersonalAccount() { }

    public static PersonalAccount Create(string name, string? phoneNumber, string? notes, string? createdBy, int tenantId)
    {
        Guard(name, phoneNumber, notes);

        return new PersonalAccount
        {
            Name = name.Trim(),
            PhoneNumber = NormalizePhone(phoneNumber),
            Notes = NormalizeNotes(notes),
            CreatedBy = createdBy,
            TenantId = tenantId,
        };
    }

    public void Update(string name, string? phoneNumber, string? notes)
    {
        Guard(name, phoneNumber, notes);
        Name = name.Trim();
        PhoneNumber = NormalizePhone(phoneNumber);
        Notes = NormalizeNotes(notes);
    }

    public void SetTenant(int tenantId) => TenantId = tenantId;

    private static string? NormalizePhone(string? phone)
        => string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();

    private static string? NormalizeNotes(string? notes)
        => string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

    private static void Guard(string name, string? phoneNumber, string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("اسم الشخص مطلوب");
        if (name.Trim().Length > 200)
            throw new DomainException("اسم الشخص طويل جداً");

        var phone = NormalizePhone(phoneNumber);
        if (phone is not null && !EgyptPhoneRegex.IsMatch(phone))
            throw new DomainException("رقم التليفون يجب أن يكون رقم مصرى صحيح (مثال: 01012345678)");

        if (notes is not null && notes.Length > 1000)
            throw new DomainException("الملاحظات طويلة جداً");
    }
}
