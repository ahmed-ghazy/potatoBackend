namespace PotatoAccounting.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public string? CreatedBy { get; protected set; }
}
