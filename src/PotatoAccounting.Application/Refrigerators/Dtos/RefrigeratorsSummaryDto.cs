namespace PotatoAccounting.Application.Refrigerators.Dtos;

public class RefrigeratorsSummaryDto
{
    public int Count { get; set; }
    public int TotalSackCount { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalExpenses { get; set; }

    /// <summary>Sacks taken out of refrigerators through trader invoices.</summary>
    public int TotalSoldSackCount { get; set; }
    /// <summary>Sacks still inside refrigerators.</summary>
    public int TotalRemainingSackCount { get; set; }
    public decimal TotalSoldWeight { get; set; }
    public decimal TotalRemainingWeight { get; set; }
}
