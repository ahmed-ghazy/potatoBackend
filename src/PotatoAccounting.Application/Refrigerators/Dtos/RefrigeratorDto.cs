using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Dtos;

public class RefrigeratorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? ReceiptImagePath { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<RefrigeratorItemDto> Items { get; set; } = new();
    public List<RefrigeratorExpenseDto> Expenses { get; set; } = new();

    public int TotalSackCount { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalExpenses { get; set; }

    /// <summary>Sacks taken out of this refrigerator through trader invoices.</summary>
    public int SoldSackCount { get; set; }
    /// <summary>Sacks still inside: TotalSackCount - SoldSackCount.</summary>
    public int RemainingSackCount { get; set; }
    /// <summary>Weight (kg) taken out of this refrigerator through trader invoices.</summary>
    public decimal SoldWeight { get; set; }
    /// <summary>Weight (kg) still inside: TotalWeight - SoldWeight.</summary>
    public decimal RemainingWeight { get; set; }

    public static RefrigeratorDto From(
        Refrigerator r,
        IEnumerable<RefrigeratorItem>? items = null,
        IEnumerable<RefrigeratorExpense>? expenses = null,
        int soldSackCount = 0,
        decimal soldWeight = 0m)
    {
        var itemList = (items ?? Enumerable.Empty<RefrigeratorItem>()).ToList();
        var expenseList = (expenses ?? Enumerable.Empty<RefrigeratorExpense>()).ToList();
        var totalSackCount = itemList.Sum(i => i.SackCount);
        var totalWeight = Math.Round(itemList.Sum(i => i.Weight), 2);
        soldWeight = Math.Round(soldWeight, 2);

        return new RefrigeratorDto
        {
            Id = r.Id,
            Name = r.Name,
            Notes = r.Notes,
            ReceiptImagePath = r.ReceiptImagePath,
            CreatedAt = r.CreatedAt,
            Items = itemList.OrderByDescending(i => i.Date).ThenByDescending(i => i.Id)
                            .Select(RefrigeratorItemDto.From).ToList(),
            Expenses = expenseList.OrderByDescending(e => e.Date).ThenByDescending(e => e.Id)
                                  .Select(RefrigeratorExpenseDto.From).ToList(),
            TotalSackCount = totalSackCount,
            TotalWeight = totalWeight,
            TotalExpenses = Math.Round(expenseList.Sum(e => e.Amount), 2),
            SoldSackCount = soldSackCount,
            RemainingSackCount = totalSackCount - soldSackCount,
            SoldWeight = soldWeight,
            RemainingWeight = Math.Round(totalWeight - soldWeight, 2),
        };
    }
}
