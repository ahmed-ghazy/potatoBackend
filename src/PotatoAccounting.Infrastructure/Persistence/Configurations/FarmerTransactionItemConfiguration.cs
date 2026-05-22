using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class FarmerTransactionItemConfiguration : IEntityTypeConfiguration<FarmerTransactionItem>
{
    public void Configure(EntityTypeBuilder<FarmerTransactionItem> b)
    {
        b.ToTable("FarmerTransactionItems");
        b.HasKey(i => i.Id);
        b.Property(i => i.FarmerTransactionId).IsRequired();
        b.Property(i => i.Description).HasMaxLength(200);
        b.Property(i => i.Weight).HasColumnType("decimal(18,2)");
        b.Property(i => i.Price).HasColumnType("decimal(18,2)");
        b.Property(i => i.ImagePath).HasMaxLength(500);
        b.Property(i => i.HasTare).IsRequired().HasDefaultValue(true);

        b.Ignore(i => i.NetWeight);
        b.Ignore(i => i.Amount);

        b.HasIndex(i => i.FarmerTransactionId);
    }
}
