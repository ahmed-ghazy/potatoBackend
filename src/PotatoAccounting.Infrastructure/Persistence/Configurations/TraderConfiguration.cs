using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class TraderConfiguration : IEntityTypeConfiguration<Trader>
{
    public void Configure(EntityTypeBuilder<Trader> b)
    {
        b.ToTable("Traders");
        b.HasKey(t => t.Id);
        b.Property(t => t.Name).IsRequired().HasMaxLength(200);
        b.Property(t => t.PhoneNumber).HasMaxLength(20);
        b.Property(t => t.TenantId).IsRequired();
        b.Property(t => t.CreatedAt);
        b.Property(t => t.CreatedBy);

        b.HasIndex(t => new { t.TenantId, t.Name }).IsUnique();
    }
}
