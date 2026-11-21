using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class RefrigeratorItemConfiguration : IEntityTypeConfiguration<RefrigeratorItem>
{
    public void Configure(EntityTypeBuilder<RefrigeratorItem> b)
    {
        b.ToTable("RefrigeratorItems");
        b.HasKey(i => i.Id);
        b.Property(i => i.RefrigeratorId).IsRequired();
        b.Property(i => i.Date).IsRequired();
        b.Property(i => i.SackCount).IsRequired();
        b.Property(i => i.Weight).HasColumnType("decimal(18,2)");
        b.Property(i => i.Description).HasMaxLength(200);
        b.Property(i => i.OwnerName).HasMaxLength(200);
        b.Property(i => i.ImagePath).HasMaxLength(500);
        b.Property(i => i.UserId);
        b.Property(i => i.TenantId).IsRequired();
        b.Property(i => i.CreatedAt);
        b.Property(i => i.CreatedBy);

        b.HasOne<Refrigerator>()
            .WithMany()
            .HasForeignKey(i => i.RefrigeratorId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(i => i.RefrigeratorId);
        b.HasIndex(i => i.Date);
        b.HasIndex(i => i.TenantId);
    }
}
