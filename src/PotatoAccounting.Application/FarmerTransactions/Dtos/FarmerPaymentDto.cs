using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.FarmerTransactions.Dtos;

public class FarmerPaymentDto
{
    public int Id { get; set; }
    public int FarmerTransactionId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }

    public static FarmerPaymentDto From(FarmerPayment p) => new()
    {
        Id = p.Id,
        FarmerTransactionId = p.FarmerTransactionId,
        Date = p.Date,
        Amount = p.Amount,
        Notes = p.Notes,
    };
}
