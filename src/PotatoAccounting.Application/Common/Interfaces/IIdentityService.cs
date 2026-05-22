using PotatoAccounting.Application.Auth.Dtos;
using PotatoAccounting.Application.Profile.Dtos;
using PotatoAccounting.Application.Users.Dtos;

namespace PotatoAccounting.Application.Common.Interfaces;

public interface IIdentityService
{
    /// <summary>
    /// Authenticates by phone number. For backward compatibility, also accepts an email
    /// (anything containing '@') and looks the user up by email instead.
    /// </summary>
    Task<AuthResponseDto> LoginAsync(string phoneNumberOrEmail, string password);

    Task<UserDto> CreateUserAsync(string phoneNumber, string password, string fullName, string role, int tenantId);
    Task<IReadOnlyList<UserDto>> GetUsersAsync(int? tenantFilter);
    Task DeleteUserAsync(string userId, string? currentUserId, int? currentTenantId, bool isSuperAdmin);

    // ===== Profile self-service =====
    Task<ProfileDto> GetMyProfileAsync(string userId);
    Task<ProfileDto> UpdateMyProfileAsync(string userId, string fullName, string phoneNumber, string? email);
    Task ChangeMyPasswordAsync(string userId, string currentPassword, string newPassword);
}
