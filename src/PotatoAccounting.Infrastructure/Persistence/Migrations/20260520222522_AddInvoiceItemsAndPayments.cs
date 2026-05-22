using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotatoAccounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceItemsAndPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Create new tables FIRST so we can backfill into them before dropping legacy columns
            migrationBuilder.CreateTable(
                name: "FarmerTransactionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerTransactionId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmerTransactionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarmerTransactionItems_FarmerTransactions_FarmerTransactionId",
                        column: x => x.FarmerTransactionId,
                        principalTable: "FarmerTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FarmerPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerTransactionId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmerPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarmerPayments_FarmerTransactions_FarmerTransactionId",
                        column: x => x.FarmerTransactionId,
                        principalTable: "FarmerTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FarmerTransactionItems_FarmerTransactionId",
                table: "FarmerTransactionItems",
                column: "FarmerTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmerPayments_FarmerTransactionId",
                table: "FarmerPayments",
                column: "FarmerTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmerPayments_TenantId",
                table: "FarmerPayments",
                column: "TenantId");

            // 2) Backfill: each existing transaction becomes one item (weight + price, no description)
            migrationBuilder.Sql(@"
                INSERT INTO FarmerTransactionItems (FarmerTransactionId, Description, Weight, Price)
                SELECT Id, NULL, Weight, Price
                FROM FarmerTransactions
                WHERE Weight > 0;
            ");

            // 3) Backfill: positive Deposit becomes a payment dated like the transaction
            migrationBuilder.Sql(@"
                INSERT INTO FarmerPayments (FarmerTransactionId, Date, Amount, Notes, UserId, TenantId, CreatedAt, CreatedBy)
                SELECT Id, Date, Deposit, N'عربون (مرحَّل من النظام القديم)', UserId, TenantId, SYSUTCDATETIME(), UserId
                FROM FarmerTransactions
                WHERE Deposit > 0;
            ");

            // 4) Backfill: if IsPaid = 1 and there was still a remaining amount, add a second payment for it
            //    Remaining at the time = ROUND(Weight/1.035 * Price, 2) - Deposit
            migrationBuilder.Sql(@"
                INSERT INTO FarmerPayments (FarmerTransactionId, Date, Amount, Notes, UserId, TenantId, CreatedAt, CreatedBy)
                SELECT Id, Date,
                       (ROUND(Weight / 1.035 * Price, 2) - Deposit) AS Amount,
                       N'سداد الباقى (مرحَّل من النظام القديم)',
                       UserId, TenantId, SYSUTCDATETIME(), UserId
                FROM FarmerTransactions
                WHERE IsPaid = 1
                  AND (ROUND(Weight / 1.035 * Price, 2) - Deposit) > 0;
            ");

            // 5) Drop legacy columns now that data is migrated
            migrationBuilder.DropColumn(name: "Deposit", table: "FarmerTransactions");
            migrationBuilder.DropColumn(name: "IsPaid", table: "FarmerTransactions");
            migrationBuilder.DropColumn(name: "Price", table: "FarmerTransactions");
            migrationBuilder.DropColumn(name: "Weight", table: "FarmerTransactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Deposit",
                table: "FarmerTransactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "FarmerTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "FarmerTransactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "FarmerTransactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Best-effort: copy the first item back into the legacy columns
            migrationBuilder.Sql(@"
                UPDATE t
                SET t.Weight = i.Weight, t.Price = i.Price
                FROM FarmerTransactions t
                INNER JOIN (
                    SELECT FarmerTransactionId, Weight, Price,
                           ROW_NUMBER() OVER (PARTITION BY FarmerTransactionId ORDER BY Id) AS rn
                    FROM FarmerTransactionItems
                ) i ON i.FarmerTransactionId = t.Id AND i.rn = 1;
            ");

            // Best-effort: sum payments back into Deposit; set IsPaid if fully covered
            migrationBuilder.Sql(@"
                UPDATE t
                SET t.Deposit = COALESCE(p.Total, 0),
                    t.IsPaid = CASE WHEN COALESCE(p.Total, 0) >= ROUND(t.Weight / 1.035 * t.Price, 2) AND t.Weight > 0 THEN 1 ELSE 0 END
                FROM FarmerTransactions t
                LEFT JOIN (
                    SELECT FarmerTransactionId, SUM(Amount) AS Total
                    FROM FarmerPayments
                    GROUP BY FarmerTransactionId
                ) p ON p.FarmerTransactionId = t.Id;
            ");

            migrationBuilder.DropTable(name: "FarmerPayments");
            migrationBuilder.DropTable(name: "FarmerTransactionItems");
        }
    }
}
