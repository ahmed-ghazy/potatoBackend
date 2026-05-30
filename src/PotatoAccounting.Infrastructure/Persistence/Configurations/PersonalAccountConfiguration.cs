using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Infrastructure.Persistence.Configurations;

public class PersonalAccountConfiguration : IEntityTypeConfiguration<PersonalAccount>
{
    public void Configure(EntityTypeBuilder<PersonalAccount> b)
    {
        b.ToTable("PersonalAccounts");
        b.HasKey(a => a.Id);
        b.Property(a => a.Name).IsRequired().HasMaxLength(200);
        b.Property(a => a.PhoneNumber).HasMaxLength(20);
        b.Property(a => a.Notes).HasMaxLength(1000);
        b.Property(a => a.TenantId).IsRequired();
        b.Property(a => a.CreatedAt);
        b.Property(a => a.CreatedBy);

        b.HasIndex(a => new { a.TenantId, a.Name }).IsUnique();
    }
}
