using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Domain.Entities;

public class FarmerTransactionItem : BaseEntity
{
    private const decimal WeightConversionFactor = 1.035m;

    public int FarmerTransactionId { get; private set; }
    public string? Description { get; private set; }
    public decimal Weight { get; private set; }
    public decimal Price { get; private set; }
    public string? ImagePath { get; private set; }
    public bool HasTare { get; private set; } = true;

    public decimal NetWeight => HasTare
        ? Math.Round(Weight / WeightConversionFactor, 2)
        : Math.Round(Weight, 2);

    public decimal Amount => Math.Round(NetWeight * Price, 2);

    private FarmerTransactionItem() { }

    public static FarmerTransactionItem Create(string? description, decimal weight, decimal price, bool hasTare = true)
    {
        Guard(description, weight, price);

        return new FarmerTransactionItem
        {
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Weight = weight,
            Price = price,
            HasTare = hasTare,
        };
    }

    public void Update(string? description, decimal weight, decimal price, bool hasTare)
    {
        Guard(description, weight, price);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Weight = weight;
        Price = price;
        HasTare = hasTare;
    }

    public void AttachImage(string? imagePath) =>
        ImagePath = string.IsNullOrWhiteSpace(imagePath) ? null : imagePath;

    public void RemoveImage() => ImagePath = null;

    internal void AttachTo(int transactionId) => FarmerTransactionId = transactionId;

    private static void Guard(string? description, decimal weight, decimal price)
    {
        if (description is not null && description.Length > 200)
            throw new DomainException("وصف العنصر طويل جداً");
        if (weight <= 0)
            throw new DomainException("الوزن يجب أن يكون أكبر من صفر");
        if (price < 0)
            throw new DomainException("السعر لا يمكن أن يكون سالباً");
    }
}
