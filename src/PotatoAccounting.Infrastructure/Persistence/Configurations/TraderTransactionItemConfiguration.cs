using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class TraderTransactionItemConfiguration : IEntityTypeConfiguration<TraderTransactionItem>
{
    public void Configure(EntityTypeBuilder<TraderTransactionItem> b)
    {
        b.ToTable("TraderTransactionItems");
        b.HasKey(i => i.Id);
        b.Property(i => i.TraderTransactionId).IsRequired();
        b.Property(i => i.Date).IsRequired();
        b.Property(i => i.Description).HasMaxLength(200);
        b.Property(i => i.Weight).HasColumnType("decimal(18,2)");
        b.Property(i => i.Price).HasColumnType("decimal(18,2)");
        b.Property(i => i.ImagePath).HasMaxLength(500);
        b.Property(i => i.HasTare).IsRequired().HasDefaultValue(true);
        b.Property(i => i.HasCommission).IsRequired().HasDefaultValue(true);
        b.Property(i => i.CommissionPerTon).HasColumnType("decimal(18,2)").HasDefaultValue(0m);

        b.Ignore(i => i.NetWeight);
        b.Ignore(i => i.Amount);
        b.Ignore(i => i.Commission);

        b.HasIndex(i => i.TraderTransactionId);
        b.HasIndex(i => i.Date);
    }
}
