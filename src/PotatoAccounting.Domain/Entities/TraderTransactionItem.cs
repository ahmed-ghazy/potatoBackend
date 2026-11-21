using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Domain.Entities;

public class TraderTransactionItem : BaseEntity
{
    private const decimal WeightConversionFactor = 1.035m;
    private const decimal KgPerTon = 1000m;

    public int TraderTransactionId { get; private set; }
    /// <summary>Optional link to the refrigerator this quantity came out of (خرج من أنهى تلاجة).</summary>
    public int? RefrigeratorId { get; private set; }
    public DateTime Date { get; private set; }
    public string? Description { get; private set; }
    public int SackCount { get; private set; }
    public decimal Weight { get; private set; }
    public decimal Price { get; private set; }
    public string? ImagePath { get; private set; }
    public bool HasTare { get; private set; } = true;

    public bool HasCommission { get; private set; } = true;
    public decimal CommissionPerTon { get; private set; }

    public decimal NetWeight => HasTare
        ? Math.Round(Weight / WeightConversionFactor, 2)
        : Math.Round(Weight, 2);

    public decimal Amount => Math.Round(NetWeight * Price, 2);

    public decimal Commission => HasCommission
        ? Math.Round((NetWeight / KgPerTon) * CommissionPerTon, 2)
        : 0m;

    private TraderTransactionItem() { }

    public static TraderTransactionItem Create(
        DateTime date,
        string? description,
        int sackCount,
        decimal weight,
        decimal price,
        bool hasTare = true,
        bool hasCommission = true,
        decimal commissionPerTon = 0m,
        int? refrigeratorId = null)
    {
        Guard(description, sackCount, weight, price, commissionPerTon);

        return new TraderTransactionItem
        {
            RefrigeratorId = NormalizeRefrigeratorId(refrigeratorId),
            Date = date,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            SackCount = sackCount,
            Weight = weight,
            Price = price,
            HasTare = hasTare,
            HasCommission = hasCommission,
            CommissionPerTon = commissionPerTon,
        };
    }

    public void Update(
        DateTime date,
        string? description,
        int sackCount,
        decimal weight,
        decimal price,
        bool hasTare,
        bool hasCommission,
        decimal commissionPerTon,
        int? refrigeratorId = null)
    {
        Guard(description, sackCount, weight, price, commissionPerTon);
        RefrigeratorId = NormalizeRefrigeratorId(refrigeratorId);
        Date = date;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SackCount = sackCount;
        Weight = weight;
        Price = price;
        HasTare = hasTare;
        HasCommission = hasCommission;
        CommissionPerTon = commissionPerTon;
    }

    public void AttachImage(string? imagePath) =>
        ImagePath = string.IsNullOrWhiteSpace(imagePath) ? null : imagePath;

    public void RemoveImage() => ImagePath = null;

    internal void AttachTo(int transactionId) => TraderTransactionId = transactionId;

    private static int? NormalizeRefrigeratorId(int? refrigeratorId)
        => refrigeratorId is > 0 ? refrigeratorId : null;

    private static void Guard(string? description, int sackCount, decimal weight, decimal price, decimal commissionPerTon)
    {
        if (description is not null && description.Length > 200)
            throw new DomainException("وصف العنصر طويل جداً");
        if (sackCount < 0)
            throw new DomainException("العدد لا يمكن أن يكون سالباً");
        if (weight <= 0)
            throw new DomainException("الوزن يجب أن يكون أكبر من صفر");
        if (price < 0)
            throw new DomainException("السعر لا يمكن أن يكون سالباً");
        if (commissionPerTon < 0)
            throw new DomainException("سعر العمولة لا يمكن أن يكون سالباً");
    }
}
