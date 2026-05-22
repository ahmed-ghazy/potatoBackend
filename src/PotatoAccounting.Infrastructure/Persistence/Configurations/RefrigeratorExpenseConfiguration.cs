using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class RefrigeratorExpenseConfiguration : IEntityTypeConfiguration<RefrigeratorExpense>
{
    public void Configure(EntityTypeBuilder<RefrigeratorExpense> b)
    {
        b.ToTable("RefrigeratorExpenses");
        b.HasKey(e => e.Id);
        b.Property(e => e.RefrigeratorId).IsRequired();
        b.Property(e => e.Date).IsRequired();
        b.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        b.Property(e => e.Description).IsRequired().HasMaxLength(200);
        b.Property(e => e.Notes).HasMaxLength(500);
        b.Property(e => e.UserId);
        b.Property(e => e.TenantId).IsRequired();
        b.Property(e => e.CreatedAt);
        b.Property(e => e.CreatedBy);

        b.HasOne<Refrigerator>()
            .WithMany()
            .HasForeignKey(e => e.RefrigeratorId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(e => e.RefrigeratorId);
        b.HasIndex(e => e.Date);
        b.HasIndex(e => e.TenantId);
    }
}
