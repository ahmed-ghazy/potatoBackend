using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Domain.Entities;

public class TraderTransaction : AuditableEntity, ITenantEntity, ISeasonAwareEntity
{
    private readonly List<TraderTransactionItem> _items = new();

    public int TraderId { get; private set; }
    public Trader? Trader { get; private set; }
    public string? Notes { get; private set; }
    public string? UserId { get; private set; }
    public int TenantId { get; private set; }
    public int SeasonId { get; private set; }

    public bool IsClosed { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public string? ClosedBy { get; private set; }

    public IReadOnlyList<TraderTransactionItem> Items => _items;

    public int TotalSackCount => _items.Sum(i => i.SackCount);
    public decimal TotalAmount => Math.Round(_items.Sum(i => i.Amount), 2);
    public decimal TotalWeight => Math.Round(_items.Sum(i => i.Weight), 2);
    public decimal TotalNetWeight => Math.Round(_items.Sum(i => i.NetWeight), 2);
    public decimal TotalCommission => Math.Round(_items.Sum(i => i.Commission), 2);

    public DateTime? LatestItemDate => _items.Count > 0 ? _items.Max(i => i.Date) : null;
    public DateTime? EarliestItemDate => _items.Count > 0 ? _items.Min(i => i.Date) : null;

    private TraderTransaction() { }

    public static TraderTransaction Create(
        int traderId,
        string? notes,
        string? userId,
        int tenantId,
        IEnumerable<TraderTransactionItem> items)
    {
        GuardHeader(traderId, notes);
        var itemList = items?.ToList() ?? new();
        GuardItems(itemList);

        var entity = new TraderTransaction
        {
            TraderId = traderId,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            UserId = userId,
            CreatedBy = userId,
            TenantId = tenantId,
        };
        foreach (var item in itemList)
            entity._items.Add(item);
        return entity;
    }

    public void UpdateHeader(int traderId, string? notes)
    {
        EnsureEditable();
        GuardHeader(traderId, notes);
        TraderId = traderId;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void AddItem(TraderTransactionItem item)
    {
        EnsureEditable();
        _items.Add(item);
    }

    public TraderTransactionItem? FindItem(int itemId) => _items.FirstOrDefault(i => i.Id == itemId);

    public List<TraderTransactionItem> RemoveItemsNotIn(IEnumerable<int> keepIds)
    {
        EnsureEditable();
        var keepSet = keepIds.ToHashSet();
        var removed = _items.Where(i => i.Id != 0 && !keepSet.Contains(i.Id)).ToList();
        foreach (var r in removed) _items.Remove(r);
        return removed;
    }

    public void EnsureHasItems()
    {
        if (_items.Count == 0)
            throw new DomainException("لا يمكن حفظ فاتورة بدون عناصر");
    }

    public void EnsureEditable()
    {
        if (IsClosed)
            throw new DomainException("الفاتورة مغلقة — لا يمكن تعديلها. افتحها أولاً.");
    }

    public void Close(string? closedBy)
    {
        if (IsClosed) return;
        EnsureHasItems();
        IsClosed = true;
        ClosedAt = DateTime.UtcNow;
        ClosedBy = closedBy;
    }

    public void Reopen()
    {
        if (!IsClosed) return;
        IsClosed = false;
        ClosedAt = null;
        ClosedBy = null;
    }

    public void SetTenant(int tenantId) => TenantId = tenantId;
    public void SetSeason(int seasonId) => SeasonId = seasonId;

    private static void GuardHeader(int traderId, string? notes)
    {
        if (traderId <= 0)
            throw new DomainException("التاجر مطلوب");
        if (notes is not null && notes.Length > 1000)
            throw new DomainException("الملاحظات طويلة جداً");
    }

    private static void GuardItems(List<TraderTransactionItem> items)
    {
        if (items.Count == 0)
            throw new DomainException("لا يمكن إنشاء فاتورة بدون عناصر");
    }
}
