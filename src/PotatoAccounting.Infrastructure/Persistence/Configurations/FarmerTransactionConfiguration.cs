using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class FarmerTransactionConfiguration : IEntityTypeConfiguration<FarmerTransaction>
{
    public void Configure(EntityTypeBuilder<FarmerTransaction> b)
    {
        b.ToTable("FarmerTransactions");
        b.HasKey(t => t.Id);
        b.Property(t => t.Date).IsRequired();
        b.Property(t => t.FarmerId).IsRequired();
        b.Property(t => t.Notes).HasMaxLength(1000);
        b.Property(t => t.UserId);
        b.Property(t => t.TenantId).IsRequired();
        b.Property(t => t.SeasonId).IsRequired();
        b.Property(t => t.CreatedAt);
        b.Property(t => t.CreatedBy);
        b.Property(t => t.IsClosed).IsRequired();
        b.Property(t => t.ClosedAt);
        b.Property(t => t.ClosedBy);

        b.HasOne(t => t.Farmer)
            .WithMany()
            .HasForeignKey(t => t.FarmerId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne<Season>()
            .WithMany()
            .HasForeignKey(t => t.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(t => t.Items)
            .WithOne()
            .HasForeignKey(i => i.FarmerTransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Metadata.FindNavigation(nameof(FarmerTransaction.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        b.Ignore(t => t.TotalAmount);
        b.Ignore(t => t.TotalWeight);
        b.Ignore(t => t.TotalNetWeight);

        b.HasIndex(t => t.UserId);
        b.HasIndex(t => t.TenantId);
        b.HasIndex(t => t.FarmerId);
        b.HasIndex(t => t.SeasonId);
    }
}
