using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class RefrigeratorConfiguration : IEntityTypeConfiguration<Refrigerator>
{
    public void Configure(EntityTypeBuilder<Refrigerator> b)
    {
        b.ToTable("Refrigerators");
        b.HasKey(r => r.Id);
        b.Property(r => r.Name).IsRequired().HasMaxLength(200);
        b.Property(r => r.Notes).HasMaxLength(1000);
        b.Property(r => r.TenantId).IsRequired();
        b.Property(r => r.SeasonId).IsRequired();
        b.Property(r => r.CreatedAt);
        b.Property(r => r.CreatedBy);

        b.HasOne<Season>()
            .WithMany()
            .HasForeignKey(r => r.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(r => new { r.TenantId, r.Name }).IsUnique();
        b.HasIndex(r => r.SeasonId);
    }
}
