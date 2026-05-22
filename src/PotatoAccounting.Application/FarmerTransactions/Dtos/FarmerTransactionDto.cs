using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.FarmerTransactions.Dtos;

public class FarmerTransactionDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int FarmerId { get; set; }
    public string FarmerName { get; set; } = string.Empty;
    public string? FarmerPhoneNumber { get; set; }
    public string? Notes { get; set; }

    public List<FarmerTransactionItemDto> Items { get; set; } = new();
    public List<FarmerPaymentDto> Payments { get; set; } = new();

    public decimal TotalWeight { get; set; }
    public decimal TotalNetWeight { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Remaining { get; set; }
    public bool IsPaid { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosedAt { get; set; }

    public static FarmerTransactionDto From(FarmerTransaction t, IEnumerable<FarmerPayment>? payments = null)
    {
        var paymentList = (payments ?? Enumerable.Empty<FarmerPayment>()).ToList();
        var totalPaid = Math.Round(paymentList.Sum(p => p.Amount), 2);
        var totalAmount = t.TotalAmount;
        // Match the UI's whole-pound display: consider invoice paid when the rounded remaining is <= 0,
        // so a tiny <1 EGP residual from net-weight rounding doesn't keep showing "متبقى".
        var remainingRounded = Math.Round(totalAmount - totalPaid, MidpointRounding.AwayFromZero);

        return new FarmerTransactionDto
        {
            Id = t.Id,
            Date = t.Date,
            FarmerId = t.FarmerId,
            FarmerName = t.Farmer?.Name ?? string.Empty,
            FarmerPhoneNumber = t.Farmer?.PhoneNumber,
            Notes = t.Notes,
            Items = t.Items.Select(FarmerTransactionItemDto.From).ToList(),
            Payments = paymentList.OrderByDescending(p => p.Date).ThenByDescending(p => p.Id)
                                  .Select(FarmerPaymentDto.From).ToList(),
            TotalWeight = t.TotalWeight,
            TotalNetWeight = t.TotalNetWeight,
            TotalAmount = totalAmount,
            TotalPaid = totalPaid,
            Remaining = remainingRounded > 0 ? remainingRounded : 0,
            IsPaid = totalAmount > 0 && remainingRounded <= 0,
            IsClosed = t.IsClosed,
            ClosedAt = t.ClosedAt,
        };
    }
}
