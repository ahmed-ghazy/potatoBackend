namespace PotatoAccounting.Application.Refrigerators.Dtos;

public class RefrigeratorsSummaryDto
{
    public int Count { get; set; }
    public int TotalSackCount { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalExpenses { get; set; }
}
