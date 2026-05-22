using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Domain.Entities;

public class RefrigeratorExpense : AuditableEntity, ITenantEntity
{
    public int RefrigeratorId { get; private set; }
    public DateTime Date { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public string? UserId { get; private set; }
    public int TenantId { get; private set; }

    private RefrigeratorExpense() { }

    public static RefrigeratorExpense Create(
        int refrigeratorId,
        DateTime date,
        decimal amount,
        string description,
        string? notes,
        string? userId,
        int tenantId)
    {
        Guard(amount, description, notes);

        return new RefrigeratorExpense
        {
            RefrigeratorId = refrigeratorId,
            Date = date,
            Amount = amount,
            Description = description.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            UserId = userId,
            CreatedBy = userId,
            TenantId = tenantId,
        };
    }

    public void Update(DateTime date, decimal amount, string description, string? notes)
    {
        Guard(amount, description, notes);
        Date = date;
        Amount = amount;
        Description = description.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void SetTenant(int tenantId) => TenantId = tenantId;

    private static void Guard(decimal amount, string description, string? notes)
    {
        if (amount <= 0)
            throw new DomainException("المبلغ يجب أن يكون أكبر من صفر");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("وصف المصروف مطلوب");
        if (description.Trim().Length > 200)
            throw new DomainException("وصف المصروف طويل جداً");
        if (notes is not null && notes.Length > 500)
            throw new DomainException("ملاحظات المصروف طويلة جداً");
    }
}
