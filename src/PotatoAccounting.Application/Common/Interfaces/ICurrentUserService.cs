namespace PotatoAccounting.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    int? TenantId { get; }
    /// <summary>
    /// Selected season for this request, read from the X-Season-Id header.
    /// null = "all seasons" view (no season filter applied).
    /// </summary>
    int? SeasonId { get; }
    bool IsAdmin { get; }
    bool IsSuperAdmin { get; }
    bool IsAuthenticated { get; }
}
