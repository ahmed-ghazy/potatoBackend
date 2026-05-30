using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class PersonalAccountEntryConfiguration : IEntityTypeConfiguration<PersonalAccountEntry>
{
    public void Configure(EntityTypeBuilder<PersonalAccountEntry> b)
    {
        b.ToTable("PersonalAccountEntries");
        b.HasKey(e => e.Id);
        b.Property(e => e.PersonalAccountId).IsRequired();
        b.Property(e => e.Date).IsRequired();
        b.Property(e => e.Direction).IsRequired().HasConversion<int>();
        b.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        b.Property(e => e.Fee).HasColumnType("decimal(18,2)");
        b.Property(e => e.PaymentMethod).HasConversion<int?>();
        b.Property(e => e.Description).HasMaxLength(500);
        b.Property(e => e.UserId);
        b.Property(e => e.TenantId).IsRequired();
        b.Property(e => e.CreatedAt);
        b.Property(e => e.CreatedBy);

        b.HasOne<PersonalAccount>()
            .WithMany()
            .HasForeignKey(e => e.PersonalAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(e => e.PersonalAccountId);
        b.HasIndex(e => e.TenantId);
    }
}
