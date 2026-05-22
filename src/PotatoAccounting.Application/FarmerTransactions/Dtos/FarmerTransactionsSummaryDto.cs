namespace PotatoAccounting.Application.FarmerTransactions.Dtos;

public class FarmerTransactionsSummaryDto
{
    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Remaining { get; set; }
    public int Count { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalNetWeight { get; set; }
}
