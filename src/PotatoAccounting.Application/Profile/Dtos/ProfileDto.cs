namespace PotatoAccounting.Application.Profile.Dtos;

public class ProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? TenantId { get; set; }
    public string? TenantName { get; set; }
}
