namespace PotatoAccounting.Application.Tenants.Dtos;

public class TenantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UsersCount { get; set; }
}
