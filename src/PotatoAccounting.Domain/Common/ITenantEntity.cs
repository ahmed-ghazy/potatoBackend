namespace PotatoAccounting.Domain.Common;

public interface ITenantEntity
{
    int TenantId { get; }
    void SetTenant(int tenantId);
}
