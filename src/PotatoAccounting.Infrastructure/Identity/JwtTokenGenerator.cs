using System.IdentityModel.Tokens.Jwt;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace PotatoAccounting.Infrastructure.Identity;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) Generate(ApplicationUser user, IList<string> roles, string? tenantName = null);
}

public class JwtTokenGenerator : IJwtTokenGenerator
{
    public const string TenantIdClaim = "tenant_id";
    public const string TenantNameClaim = "tenant_name";

    private readonly JwtSettings _settings;

    public JwtTokenGenerator(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public (string Token, DateTime ExpiresAt) Generate(ApplicationUser user, IList<string> roles, string? tenantName = null)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("FullName", user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (user.TenantId.HasValue)
            claims.Add(new Claim(TenantIdClaim, user.TenantId.Value.ToString(CultureInfo.InvariantCulture)));

        if (!string.IsNullOrEmpty(tenantName))
            claims.Add(new Claim(TenantNameClaim, tenantName));

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
