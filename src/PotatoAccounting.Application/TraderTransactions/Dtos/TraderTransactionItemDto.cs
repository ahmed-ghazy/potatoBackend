using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.TraderTransactions.Dtos;

public class TraderTransactionItemDto
{
    public int Id { get; set; }
    public int? RefrigeratorId { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public int SackCount { get; set; }
    public decimal Weight { get; set; }
    public decimal Price { get; set; }
    public string? ImagePath { get; set; }
    public bool HasTare { get; set; }
    public bool HasCommission { get; set; }
    public decimal CommissionPerTon { get; set; }

    public decimal NetWeight { get; set; }
    public decimal Amount { get; set; }
    public decimal Commission { get; set; }

    public static TraderTransactionItemDto From(TraderTransactionItem i) => new()
    {
        Id = i.Id,
        RefrigeratorId = i.RefrigeratorId,
        Date = i.Date,
        Description = i.Description,
        SackCount = i.SackCount,
        Weight = i.Weight,
        Price = i.Price,
        ImagePath = i.ImagePath,
        HasTare = i.HasTare,
        HasCommission = i.HasCommission,
        CommissionPerTon = i.CommissionPerTon,
        NetWeight = i.NetWeight,
        Amount = i.Amount,
        Commission = i.Commission,
    };
}
