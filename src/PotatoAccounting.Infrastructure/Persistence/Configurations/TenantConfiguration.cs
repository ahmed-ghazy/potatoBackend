using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("Tenants");
        b.HasKey(t => t.Id);
        b.Property(t => t.Name).IsRequired().HasMaxLength(200);
        b.Property(t => t.IsActive);
        b.Property(t => t.CreatedAt);
        b.Property(t => t.CreatedBy);

        b.HasIndex(t => t.Name).IsUnique();
    }
}
