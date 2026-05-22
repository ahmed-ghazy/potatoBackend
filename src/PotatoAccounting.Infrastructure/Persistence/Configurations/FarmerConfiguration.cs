using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class FarmerConfiguration : IEntityTypeConfiguration<Farmer>
{
    public void Configure(EntityTypeBuilder<Farmer> b)
    {
        b.ToTable("Farmers");
        b.HasKey(f => f.Id);
        b.Property(f => f.Name).IsRequired().HasMaxLength(200);
        b.Property(f => f.PhoneNumber).HasMaxLength(20);
        b.Property(f => f.TenantId).IsRequired();
        b.Property(f => f.CreatedAt);
        b.Property(f => f.CreatedBy);

        b.HasIndex(f => new { f.TenantId, f.Name }).IsUnique();
    }
}
