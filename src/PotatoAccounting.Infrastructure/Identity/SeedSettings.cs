namespace PotatoAccounting.Infrastructure.Identity;

public class DefaultSuperAdminSettings
{
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class DefaultTenantSettings
{
    public string Name { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPhoneNumber { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminFullName { get; set; } = string.Empty;
}
