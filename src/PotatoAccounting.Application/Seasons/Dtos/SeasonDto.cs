using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Seasons.Dtos;

public class SeasonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }

    public static SeasonDto From(Season s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Notes = s.Notes,
        IsDefault = s.IsDefault,
        CreatedAt = s.CreatedAt,
    };
}
