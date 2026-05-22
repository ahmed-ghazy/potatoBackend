using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> b)
    {
        b.ToTable("Seasons");
        b.HasKey(s => s.Id);
        b.Property(s => s.Name).IsRequired().HasMaxLength(200);
        b.Property(s => s.Notes).HasMaxLength(1000);
        b.Property(s => s.IsDefault).IsRequired();
        b.Property(s => s.TenantId).IsRequired();
        b.Property(s => s.CreatedAt);
        b.Property(s => s.CreatedBy);

        b.HasIndex(s => new { s.TenantId, s.Name }).IsUnique();
        // One default season per tenant.
        b.HasIndex(s => s.TenantId)
            .HasFilter("[IsDefault] = 1")
            .IsUnique()
            .HasDatabaseName("IX_Seasons_TenantId_Default");
    }
}
