using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Dtos;

public class RefrigeratorExpenseDto
{
    public int Id { get; set; }
    public int RefrigeratorId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public static RefrigeratorExpenseDto From(RefrigeratorExpense e) => new()
    {
        Id = e.Id,
        RefrigeratorId = e.RefrigeratorId,
        Date = e.Date,
        Amount = e.Amount,
        Description = e.Description,
        Notes = e.Notes,
    };
}
