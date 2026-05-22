using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class TraderPaymentConfiguration : IEntityTypeConfiguration<TraderPayment>
{
    public void Configure(EntityTypeBuilder<TraderPayment> b)
    {
        b.ToTable("TraderPayments");
        b.HasKey(p => p.Id);
        b.Property(p => p.TraderTransactionId).IsRequired();
        b.Property(p => p.Date).IsRequired();
        b.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        b.Property(p => p.Notes).HasMaxLength(500);
        b.Property(p => p.UserId);
        b.Property(p => p.TenantId).IsRequired();
        b.Property(p => p.CreatedAt);
        b.Property(p => p.CreatedBy);

        b.HasOne<TraderTransaction>()
            .WithMany()
            .HasForeignKey(p => p.TraderTransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(p => p.TraderTransactionId);
        b.HasIndex(p => p.TenantId);
    }
}
