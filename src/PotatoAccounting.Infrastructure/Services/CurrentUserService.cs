using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Infrastructure.Identity;

namespace PotatoAccounting.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public string? UserId =>
        Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Principal?.FindFirstValue("sub");

    public int? TenantId
    {
        get
        {
            var raw = Principal?.FindFirstValue(JwtTokenGenerator.TenantIdClaim);
            if (string.IsNullOrEmpty(raw)) return null;
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;
        }
    }

    public int? SeasonId
    {
        get
        {
            var raw = _accessor.HttpContext?.Request.Headers["X-Season-Id"].ToString();
            if (string.IsNullOrEmpty(raw)) return null;
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) && v > 0 ? v : null;
        }
    }

    public bool IsAdmin => Principal?.IsInRole("Admin") ?? false;
    public bool IsSuperAdmin => Principal?.IsInRole("SuperAdmin") ?? false;
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}
