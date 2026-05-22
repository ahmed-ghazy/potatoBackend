using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Dtos;

public class RefrigeratorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<RefrigeratorItemDto> Items { get; set; } = new();
    public List<RefrigeratorExpenseDto> Expenses { get; set; } = new();

    public int TotalSackCount { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal TotalExpenses { get; set; }

    public static RefrigeratorDto From(
        Refrigerator r,
        IEnumerable<RefrigeratorItem>? items = null,
        IEnumerable<RefrigeratorExpense>? expenses = null)
    {
        var itemList = (items ?? Enumerable.Empty<RefrigeratorItem>()).ToList();
        var expenseList = (expenses ?? Enumerable.Empty<RefrigeratorExpense>()).ToList();

        return new RefrigeratorDto
        {
            Id = r.Id,
            Name = r.Name,
            Notes = r.Notes,
            CreatedAt = r.CreatedAt,
            Items = itemList.OrderByDescending(i => i.Date).ThenByDescending(i => i.Id)
                            .Select(RefrigeratorItemDto.From).ToList(),
            Expenses = expenseList.OrderByDescending(e => e.Date).ThenByDescending(e => e.Id)
                                  .Select(RefrigeratorExpenseDto.From).ToList(),
            TotalSackCount = itemList.Sum(i => i.SackCount),
            TotalWeight = Math.Round(itemList.Sum(i => i.Weight), 2),
            TotalExpenses = Math.Round(expenseList.Sum(e => e.Amount), 2),
        };
    }
}
