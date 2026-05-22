namespace PotatoAccounting.Application.TraderTransactions.Dtos;

public class TraderTransactionsSummaryDto
{
    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Remaining { get; set; }
    public decimal TotalCommission { get; set; }
    public int Count { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalNetWeight { get; set; }
}
