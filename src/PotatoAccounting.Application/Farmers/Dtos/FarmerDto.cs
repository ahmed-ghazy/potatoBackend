using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Farmers.Dtos;

public class FarmerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    public static FarmerDto From(Farmer f) => new()
    {
        Id = f.Id,
        Name = f.Name,
        PhoneNumber = f.PhoneNumber,
        CreatedAt = f.CreatedAt,
    };
}
