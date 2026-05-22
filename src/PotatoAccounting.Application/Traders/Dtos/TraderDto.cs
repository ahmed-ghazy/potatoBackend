using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Traders.Dtos;

public class TraderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    public static TraderDto From(Trader t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        PhoneNumber = t.PhoneNumber,
        CreatedAt = t.CreatedAt,
    };
}
