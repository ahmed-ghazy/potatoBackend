using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.FarmerTransactions.Dtos;

public class FarmerTransactionItemDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public decimal Weight { get; set; }
    public decimal Price { get; set; }
    public string? ImagePath { get; set; }
    public bool HasTare { get; set; }
    public decimal NetWeight { get; set; }
    public decimal Amount { get; set; }

    public static FarmerTransactionItemDto From(FarmerTransactionItem i) => new()
    {
        Id = i.Id,
        Description = i.Description,
        Weight = i.Weight,
        Price = i.Price,
        ImagePath = i.ImagePath,
        HasTare = i.HasTare,
        NetWeight = i.NetWeight,
        Amount = i.Amount,
    };
}
