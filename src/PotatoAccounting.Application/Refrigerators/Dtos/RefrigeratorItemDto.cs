using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Refrigerators.Dtos;

public class RefrigeratorItemDto
{
    public int Id { get; set; }
    public int RefrigeratorId { get; set; }
    public DateTime Date { get; set; }
    public int SackCount { get; set; }
    public decimal Weight { get; set; }
    public string? Description { get; set; }

    public static RefrigeratorItemDto From(RefrigeratorItem i) => new()
    {
        Id = i.Id,
        RefrigeratorId = i.RefrigeratorId,
        Date = i.Date,
        SackCount = i.SackCount,
        Weight = i.Weight,
        Description = i.Description,
    };
}
