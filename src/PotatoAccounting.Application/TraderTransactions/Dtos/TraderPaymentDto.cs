using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.TraderTransactions.Dtos;

public class TraderPaymentDto
{
    public int Id { get; set; }
    public int TraderTransactionId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }

    public static TraderPaymentDto From(TraderPayment p) => new()
    {
        Id = p.Id,
        TraderTransactionId = p.TraderTransactionId,
        Date = p.Date,
        Amount = p.Amount,
        Notes = p.Notes,
    };
}
