using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class TraderTransactionConfiguration : IEntityTypeConfiguration<TraderTransaction>
{
    public void Configure(EntityTypeBuilder<TraderTransaction> b)
    {
        b.ToTable("TraderTransactions");
        b.HasKey(t => t.Id);
        b.Property(t => t.TraderId).IsRequired();
        b.Property(t => t.Notes).HasMaxLength(1000);
        b.Property(t => t.UserId);
        b.Property(t => t.TenantId).IsRequired();
        b.Property(t => t.SeasonId).IsRequired();
        b.Property(t => t.CreatedAt);
        b.Property(t => t.CreatedBy);
        b.Property(t => t.IsClosed).IsRequired();
        b.Property(t => t.ClosedAt);
        b.Property(t => t.ClosedBy);

        b.HasOne(t => t.Trader)
            .WithMany()
            .HasForeignKey(t => t.TraderId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne<Season>()
            .WithMany()
            .HasForeignKey(t => t.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(t => t.Items)
            .WithOne()
            .HasForeignKey(i => i.TraderTransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Metadata.FindNavigation(nameof(TraderTransaction.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        b.Ignore(t => t.TotalAmount);
        b.Ignore(t => t.TotalSackCount);
        b.Ignore(t => t.TotalWeight);
        b.Ignore(t => t.TotalNetWeight);
        b.Ignore(t => t.TotalCommission);
        b.Ignore(t => t.LatestItemDate);
        b.Ignore(t => t.EarliestItemDate);

        b.HasIndex(t => t.UserId);
        b.HasIndex(t => t.TenantId);
        b.HasIndex(t => t.TraderId);
        b.HasIndex(t => t.SeasonId);
    }
}
