using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.TraderTransactions.Dtos;

public class TraderTransactionDto
{
    public int Id { get; set; }
    public int TraderId { get; set; }
    public string TraderName { get; set; } = string.Empty;
    public string? TraderPhoneNumber { get; set; }
    public string? Notes { get; set; }

    public List<TraderTransactionItemDto> Items { get; set; } = new();
    public List<TraderPaymentDto> Payments { get; set; } = new();

    public int TotalSackCount { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalNetWeight { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Remaining { get; set; }
    public bool IsPaid { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosedAt { get; set; }

    public DateTime? LatestItemDate { get; set; }
    public DateTime? EarliestItemDate { get; set; }

    public static TraderTransactionDto From(TraderTransaction t, IEnumerable<TraderPayment>? payments = null)
    {
        var paymentList = (payments ?? Enumerable.Empty<TraderPayment>()).ToList();
        var totalPaid = Math.Round(paymentList.Sum(p => p.Amount), 2);
        var totalAmount = t.TotalAmount;
        var remainingRounded = Math.Round(totalAmount - totalPaid, MidpointRounding.AwayFromZero);

        return new TraderTransactionDto
        {
            Id = t.Id,
            TraderId = t.TraderId,
            TraderName = t.Trader?.Name ?? string.Empty,
            TraderPhoneNumber = t.Trader?.PhoneNumber,
            Notes = t.Notes,
            Items = t.Items.OrderBy(i => i.Date).ThenBy(i => i.Id).Select(TraderTransactionItemDto.From).ToList(),
            Payments = paymentList.OrderByDescending(p => p.Date).ThenByDescending(p => p.Id)
                                  .Select(TraderPaymentDto.From).ToList(),
            TotalSackCount = t.TotalSackCount,
            TotalWeight = t.TotalWeight,
            TotalNetWeight = t.TotalNetWeight,
            TotalAmount = totalAmount,
            TotalCommission = t.TotalCommission,
            TotalPaid = totalPaid,
            Remaining = remainingRounded > 0 ? remainingRounded : 0,
            IsPaid = totalAmount > 0 && remainingRounded <= 0,
            IsClosed = t.IsClosed,
            ClosedAt = t.ClosedAt,
            LatestItemDate = t.LatestItemDate,
            EarliestItemDate = t.EarliestItemDate,
        };
    }
}
