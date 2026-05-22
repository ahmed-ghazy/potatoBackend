using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Domain.Entities;

public class TraderPayment : AuditableEntity, ITenantEntity
{
    public int TraderTransactionId { get; private set; }
    public DateTime Date { get; private set; }
    public decimal Amount { get; private set; }
    public string? Notes { get; private set; }
    public string? UserId { get; private set; }
    public int TenantId { get; private set; }

    private TraderPayment() { }

    public static TraderPayment Create(
        int traderTransactionId,
        DateTime date,
        decimal amount,
        string? notes,
        string? userId,
        int tenantId)
    {
        Guard(amount, notes);

        return new TraderPayment
        {
            TraderTransactionId = traderTransactionId,
            Date = date,
            Amount = amount,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            UserId = userId,
            CreatedBy = userId,
            TenantId = tenantId,
        };
    }

    public void Update(DateTime date, decimal amount, string? notes)
    {
        Guard(amount, notes);
        Date = date;
        Amount = amount;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void SetTenant(int tenantId) => TenantId = tenantId;

    private static void Guard(decimal amount, string? notes)
    {
        if (amount <= 0)
            throw new DomainException("مبلغ الدفعة يجب أن يكون أكبر من صفر");
        if (notes is not null && notes.Length > 500)
            throw new DomainException("ملاحظات الدفعة طويلة جداً");
    }
}
