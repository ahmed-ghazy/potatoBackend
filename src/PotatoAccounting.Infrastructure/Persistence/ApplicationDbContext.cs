using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Application.Common.Interfaces;
using PotatoAccounting.Domain.Common;
using PotatoAccounting.Domain.Entities;
using PotatoAccounting.Infrastructure.Identity;

namespace PotatoAccounting.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUser;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<FarmerTransaction> FarmerTransactions => Set<FarmerTransaction>();
    public DbSet<FarmerTransactionItem> FarmerTransactionItems => Set<FarmerTransactionItem>();
    public DbSet<FarmerPayment> FarmerPayments => Set<FarmerPayment>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Farmer> Farmers => Set<Farmer>();
    public DbSet<Trader> Traders => Set<Trader>();
    public DbSet<TraderTransaction> TraderTransactions => Set<TraderTransaction>();
    public DbSet<TraderTransactionItem> TraderTransactionItems => Set<TraderTransactionItem>();
    public DbSet<TraderPayment> TraderPayments => Set<TraderPayment>();
    public DbSet<Refrigerator> Refrigerators => Set<Refrigerator>();
    public DbSet<RefrigeratorItem> RefrigeratorItems => Set<RefrigeratorItem>();
    public DbSet<RefrigeratorExpense> RefrigeratorExpenses => Set<RefrigeratorExpense>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<PersonalAccount> PersonalAccounts => Set<PersonalAccount>();
    public DbSet<PersonalAccountEntry> PersonalAccountEntries => Set<PersonalAccountEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters — SuperAdmin sees all, others see only their tenant.
        // For season-aware entities, an additional filter narrows to the selected season
        // when the request carries X-Season-Id; otherwise all seasons are visible.
        builder.Entity<Expense>().HasQueryFilter(e =>
            _currentUser.IsSuperAdmin
            || (_currentUser.TenantId != null && e.TenantId == _currentUser.TenantId
                && (_currentUser.SeasonId == null || e.SeasonId == _currentUser.SeasonId)));

        builder.Entity<FarmerTransaction>().HasQueryFilter(t =>
            _currentUser.IsSuperAdmin
            || (_currentUser.TenantId != null && t.TenantId == _currentUser.TenantId
                && (_currentUser.SeasonId == null || t.SeasonId == _currentUser.SeasonId)));

        builder.Entity<Farmer>().HasQueryFilter(f =>
            _currentUser.IsSuperAdmin
            || (_currentUser.TenantId != null && f.TenantId == _currentUser.TenantId));

        builder.Entity<FarmerPayment>().HasQueryFilter(p =>
            _currentUser.IsSuperAdmin
            || (_currentUser.TenantId != null && p.TenantId == _currentUser.TenantId));

        builder.Entity<Trader>().HasQueryFilter(t =>
            _currentUser.IsSuperAdmin
            || (_currentUser.TenantId != null && t.TenantId == _currentUser.TenantId));

        builder.Entity<TraderTransaction>().HasQueryFilter(t =>
            _currentUser.IsSuperAdmin
            || (_currentUser.TenantId != null && t.TenantId == _currentUser.TenantId
                && (_currentUser.SeasonId == null || t.SeasonId == _currentUser.SeasonId)));

        builder.Entity<TraderPayment>().HasQueryFilter(p =>
            _currentUser.IsSuperAdmin
            || (_currentUser.TenantId != null && p.TenantId == _currentUser.TenantId));

        builder.Entity<Refrigerator>().HasQueryFilter(r =>
            _currentUser.IsSuperAdmin
            || (_currentUser.TenantId != null && r.TenantId == _currentUser.TenantId
                && (_currentUser.SeasonId == null || r.SeasonId == _currentUser.SeasonId)));

        builder.Entity<RefrigeratorItem>().HasQueryFilter(i =>
            _currentUser.IsSuperAdmin
            || (_currentUser.TenantId != null && i.TenantId == _currentUser.TenantId));

        builder.Entity<RefrigeratorExpense>().HasQueryFilter(e =>
            _currentUser.IsSuperAdmin
            || (_currentUser.TenantId != null && e.TenantId == _currentUser.TenantId));

        builder.Entity<Season>().HasQueryFilter(s =>
            _currentUser.IsSuperAdmin
            || (_currentUser.TenantId != null && s.TenantId == _currentUser.TenantId));

        builder.Entity<PersonalAccount>().HasQueryFilter(a =>
            _currentUser.IsSuperAdmin
            || (_currentUser.TenantId != null && a.TenantId == _currentUser.TenantId));

        builder.Entity<PersonalAccountEntry>().HasQueryFilter(e =>
            _currentUser.IsSuperAdmin
            || (_currentUser.TenantId != null && e.TenantId == _currentUser.TenantId));
    }

    public override int SaveChanges()
    {
        StampTenant();
        StampSeason();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampTenant();
        StampSeason();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampTenant()
    {
        if (_currentUser.TenantId is not int tenantId) return;

        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == 0)
                entry.Entity.SetTenant(tenantId);
        }
    }

    private void StampSeason()
    {
        if (_currentUser.TenantId is not int tenantId) return;

        var pending = ChangeTracker.Entries<ISeasonAwareEntity>()
            .Where(e => e.State == EntityState.Added && e.Entity.SeasonId == 0)
            .ToList();
        if (pending.Count == 0) return;

        // Header-selected season takes priority; otherwise fall back to the tenant's default.
        int? seasonId = _currentUser.SeasonId;
        if (seasonId is null)
        {
            // EF query filters apply tenant scope automatically here.
            seasonId = Seasons
                .Where(s => s.IsDefault)
                .Select(s => (int?)s.Id)
                .FirstOrDefault();
        }

        if (seasonId is not int sid)
            throw new InvalidOperationException(
                "لا يوجد موسم محدد ولا موسم افتراضى لهذه الشركة — لا يمكن إنشاء السجل");

        foreach (var entry in pending)
            entry.Entity.SetSeason(sid);
    }
}
