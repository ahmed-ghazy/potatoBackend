using Microsoft.EntityFrameworkCore;
using PotatoAccounting.Domain.Entities;

namespace PotatoAccounting.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<FarmerTransaction> FarmerTransactions { get; }
    DbSet<FarmerTransactionItem> FarmerTransactionItems { get; }
    DbSet<FarmerPayment> FarmerPayments { get; }
    DbSet<Expense> Expenses { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<Farmer> Farmers { get; }
    DbSet<Trader> Traders { get; }
    DbSet<TraderTransaction> TraderTransactions { get; }
    DbSet<TraderTransactionItem> TraderTransactionItems { get; }
    DbSet<TraderPayment> TraderPayments { get; }
    DbSet<Refrigerator> Refrigerators { get; }
    DbSet<RefrigeratorItem> RefrigeratorItems { get; }
    DbSet<RefrigeratorExpense> RefrigeratorExpenses { get; }
    DbSet<Season> Seasons { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
