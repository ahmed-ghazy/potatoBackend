using Microsoft.AspNetCore.Identity;

namespace PotatoAccounting.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tenant this user belongs to. Null for SuperAdmin (cross-tenant).
    /// </summary>
    public int? TenantId { get; set; }
}
