using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Auth.Dtos;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Application.Profile.Dtos;
using PotatoAccounting.Application.Users.Dtos;
using PotatoAccounting.Domain.Entities;
using PotatoAccounting.Infrastructure.Persistence;

namespace PotatoAccounting.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwt;
    private readonly ApplicationDbContext _db;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwt,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwt = jwt;
        _db = db;
    }

    public async Task<AuthResponseDto> LoginAsync(string phoneNumberOrEmail, string password)
    {
        var user = await FindUserForLoginAsync(phoneNumberOrEmail)
            ?? throw new ForbiddenAccessException("بيانات الدخول غير صحيحة");

        if (!await _userManager.CheckPasswordAsync(user, password))
            throw new ForbiddenAccessException("بيانات الدخول غير صحيحة");

        var roles = await _userManager.GetRolesAsync(user);
        var tenant = user.TenantId.HasValue
            ? await _db.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == user.TenantId.Value)
            : null;

        if (tenant is not null && !tenant.IsActive)
            throw new ForbiddenAccessException("الشركة غير نشطة، تواصل مع المسؤول");

        var (token, expiresAt) = _jwt.Generate(user, roles, tenant?.Name);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? "User",
            TenantId = user.TenantId,
            TenantName = tenant?.Name,
            ExpiresAt = expiresAt,
        };
    }

    private async Task<ApplicationUser?> FindUserForLoginAsync(string identifier)
    {
        var trimmed = identifier.Trim();
        // Looks like an email? Try the email path first — covers legacy users seeded by email.
        if (trimmed.Contains('@'))
        {
            var byEmail = await _userManager.FindByEmailAsync(trimmed);
            if (byEmail is not null) return byEmail;
        }

        // Otherwise (or if email lookup didn't match), try phone — first via UserName,
        // then via a direct PhoneNumber column scan to be lenient with how the user was created.
        var byUserName = await _userManager.FindByNameAsync(trimmed);
        if (byUserName is not null) return byUserName;

        return await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == trimmed);
    }

    public async Task<UserDto> CreateUserAsync(string phoneNumber, string password, string fullName, string role, int tenantId)
    {
        var tenant = await _db.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId)
            ?? throw new ValidationException(new[] { new ValidationFailure("tenantId", "الشركة غير موجودة") });

        var trimmedPhone = phoneNumber.Trim();
        if (string.IsNullOrEmpty(trimmedPhone))
            throw new ValidationException(new[] { new ValidationFailure(nameof(phoneNumber), "رقم التليفون مطلوب") });

        // UserName must be unique across all users — we use the phone for that.
        if (await _userManager.FindByNameAsync(trimmedPhone) is not null
            || await _userManager.Users.AnyAsync(u => u.PhoneNumber == trimmedPhone))
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(phoneNumber), "رقم التليفون مسجل بالفعل") });
        }

        var user = new ApplicationUser
        {
            UserName = trimmedPhone,
            PhoneNumber = trimmedPhone,
            PhoneNumberConfirmed = true,
            FullName = fullName,
            EmailConfirmed = true,
            TenantId = tenantId,
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var failures = result.Errors.Select(e => new ValidationFailure(nameof(password), e.Description));
            throw new ValidationException(failures);
        }

        var roleName = role == "Admin" ? "Admin" : "User";
        if (!await _roleManager.RoleExistsAsync(roleName))
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        await _userManager.AddToRoleAsync(user, roleName);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            FullName = user.FullName,
            Role = roleName,
            TenantId = tenantId,
            TenantName = tenant.Name,
            CreatedAt = user.CreatedAt,
        };
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(int? tenantFilter)
    {
        var query = _userManager.Users.AsNoTracking();
        if (tenantFilter.HasValue)
            query = query.Where(u => u.TenantId == tenantFilter.Value);

        var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

        var tenantIds = users.Where(u => u.TenantId.HasValue).Select(u => u.TenantId!.Value).Distinct().ToList();
        var tenantMap = await _db.Tenants.IgnoreQueryFilters()
            .Where(t => tenantIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name);

        var result = new List<UserDto>(users.Count);
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new UserDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                FullName = u.FullName,
                Role = roles.FirstOrDefault() ?? "User",
                TenantId = u.TenantId,
                TenantName = u.TenantId.HasValue && tenantMap.TryGetValue(u.TenantId.Value, out var n) ? n : null,
                CreatedAt = u.CreatedAt,
            });
        }
        return result;
    }

    public async Task DeleteUserAsync(string userId, string? currentUserId, int? currentTenantId, bool isSuperAdmin)
    {
        if (!string.IsNullOrEmpty(currentUserId) && userId == currentUserId)
            throw new ValidationException(new[] { new ValidationFailure("id", "لا يمكنك حذف حسابك الخاص") });

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        if (!isSuperAdmin && user.TenantId != currentTenantId)
            throw new ForbiddenAccessException();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new ValidationFailure("id", e.Description));
            throw new ValidationException(errors);
        }
    }

    public async Task<ProfileDto> GetMyProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var roles = await _userManager.GetRolesAsync(user);
        var tenant = user.TenantId.HasValue
            ? await _db.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == user.TenantId.Value)
            : null;

        return new ProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? "User",
            TenantId = user.TenantId,
            TenantName = tenant?.Name,
        };
    }

    public async Task<ProfileDto> UpdateMyProfileAsync(string userId, string fullName, string phoneNumber, string? email)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var trimmedPhone = phoneNumber.Trim();
        var trimmedEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

        if (string.IsNullOrEmpty(trimmedPhone))
            throw new ValidationException(new[] { new ValidationFailure(nameof(phoneNumber), "رقم التليفون مطلوب") });

        // If the phone changed, check it isn't already taken by another user.
        if (!string.Equals(user.PhoneNumber, trimmedPhone, StringComparison.Ordinal))
        {
            var clash = await _userManager.Users.AnyAsync(u =>
                u.Id != user.Id &&
                (u.PhoneNumber == trimmedPhone || u.UserName == trimmedPhone));
            if (clash)
                throw new ValidationException(new[] { new ValidationFailure(nameof(phoneNumber), "رقم التليفون مسجل بالفعل لمستخدم آخر") });
        }

        user.FullName = fullName.Trim();
        user.PhoneNumber = trimmedPhone;
        user.PhoneNumberConfirmed = true;
        // Keep UserName aligned with the phone so login-by-phone keeps working.
        user.UserName = trimmedPhone;
        user.NormalizedUserName = _userManager.NormalizeName(trimmedPhone);

        if (trimmedEmail is not null)
        {
            user.Email = trimmedEmail;
            user.NormalizedEmail = _userManager.NormalizeEmail(trimmedEmail);
            user.EmailConfirmed = true;
        }
        else
        {
            user.Email = null;
            user.NormalizedEmail = null;
            user.EmailConfirmed = false;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new ValidationFailure(nameof(phoneNumber), e.Description));
            throw new ValidationException(errors);
        }

        return await GetMyProfileAsync(userId);
    }

    public async Task ChangeMyPasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        if (!await _userManager.CheckPasswordAsync(user, currentPassword))
            throw new ValidationException(new[] { new ValidationFailure(nameof(currentPassword), "كلمة المرور الحالية غير صحيحة") });

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new ValidationFailure(nameof(newPassword), e.Description));
            throw new ValidationException(errors);
        }
    }
}
