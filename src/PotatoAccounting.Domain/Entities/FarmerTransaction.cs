using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Domain.Entities;

public class FarmerTransaction : AuditableEntity, ITenantEntity, ISeasonAwareEntity
{
    private readonly List<FarmerTransactionItem> _items = new();

    public DateTime Date { get; private set; }
    public int FarmerId { get; private set; }
    public Farmer? Farmer { get; private set; }
    public string? Notes { get; private set; }
    public string? UserId { get; private set; }
    public int TenantId { get; private set; }
    public int SeasonId { get; private set; }

    public bool IsClosed { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public string? ClosedBy { get; private set; }

    public IReadOnlyList<FarmerTransactionItem> Items => _items;

    public decimal TotalAmount => Math.Round(_items.Sum(i => i.Amount), 2);
    public decimal TotalWeight => Math.Round(_items.Sum(i => i.Weight), 2);
    public decimal TotalNetWeight => Math.Round(_items.Sum(i => i.NetWeight), 2);

    private FarmerTransaction() { }

    public static FarmerTransaction Create(
        DateTime date,
        int farmerId,
        string? notes,
        string? userId,
        int tenantId,
        IEnumerable<FarmerTransactionItem> items)
    {
        GuardHeader(farmerId, notes);
        var itemList = items?.ToList() ?? new();

        var entity = new FarmerTransaction
        {
            Date = date,
            FarmerId = farmerId,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            UserId = userId,
            CreatedBy = userId,
            TenantId = tenantId,
        };
        foreach (var item in itemList)
            entity._items.Add(item);
        return entity;
    }

    public void UpdateHeader(DateTime date, int farmerId, string? notes)
    {
        EnsureEditable();
        GuardHeader(farmerId, notes);
        Date = date;
        FarmerId = farmerId;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void AddItem(FarmerTransactionItem item)
    {
        EnsureEditable();
        _items.Add(item);
    }

    public FarmerTransactionItem? FindItem(int itemId) => _items.FirstOrDefault(i => i.Id == itemId);

    public List<FarmerTransactionItem> RemoveItemsNotIn(IEnumerable<int> keepIds)
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
            throw new DomainException("لا يمكن إغلاق فاتورة بدون عناصر");
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

    private static void GuardHeader(int farmerId, string? notes)
    {
        if (farmerId <= 0)
            throw new DomainException("الفلاح مطلوب");
        if (notes is not null && notes.Length > 1000)
            throw new DomainException("الملاحظات طويلة جداً");
    }
}
