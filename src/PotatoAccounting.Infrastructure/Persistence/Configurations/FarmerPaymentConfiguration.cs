using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class FarmerPaymentConfiguration : IEntityTypeConfiguration<FarmerPayment>
{
    public void Configure(EntityTypeBuilder<FarmerPayment> b)
    {
        b.ToTable("FarmerPayments");
        b.HasKey(p => p.Id);
        b.Property(p => p.FarmerTransactionId).IsRequired();
        b.Property(p => p.Date).IsRequired();
        b.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        b.Property(p => p.Notes).HasMaxLength(500);
        b.Property(p => p.UserId);
        b.Property(p => p.TenantId).IsRequired();
        b.Property(p => p.CreatedAt);
        b.Property(p => p.CreatedBy);

        b.HasOne<FarmerTransaction>()
            .WithMany()
            .HasForeignKey(p => p.FarmerTransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(p => p.FarmerTransactionId);
        b.HasIndex(p => p.TenantId);
    }
}
