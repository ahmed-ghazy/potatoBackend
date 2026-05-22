using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> b)
    {
        b.ToTable("Expenses");
        b.HasKey(e => e.Id);
        b.Property(e => e.Date).IsRequired();
        b.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        b.Property(e => e.Description).IsRequired().HasMaxLength(500);
        b.Property(e => e.UserId);
        b.Property(e => e.TenantId).IsRequired();
        b.Property(e => e.SeasonId).IsRequired();
        b.Property(e => e.CreatedAt);
        b.Property(e => e.CreatedBy);

        b.HasOne<Season>()
            .WithMany()
            .HasForeignKey(e => e.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(e => e.UserId);
        b.HasIndex(e => e.TenantId);
        b.HasIndex(e => e.SeasonId);
    }
}
