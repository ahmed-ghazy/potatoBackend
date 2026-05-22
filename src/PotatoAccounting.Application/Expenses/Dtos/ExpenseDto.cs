using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Expenses.Dtos;

public class ExpenseDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;

    public static ExpenseDto From(Expense e) => new()
    {
        Id = e.Id,
        Date = e.Date,
        Amount = e.Amount,
        Description = e.Description,
    };
}
