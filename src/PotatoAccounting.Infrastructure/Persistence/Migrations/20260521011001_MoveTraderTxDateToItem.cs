using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotatoAccounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveTraderTxDateToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add Date column to items as nullable so we can backfill
            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "TraderTransactionItems",
                type: "datetime2",
                nullable: true);

            // 2) Backfill: copy header date to all items
            migrationBuilder.Sql(@"
                UPDATE i
                SET i.Date = t.Date
                FROM TraderTransactionItems i
                INNER JOIN TraderTransactions t ON t.Id = i.TraderTransactionId;
            ");

            // 3) Items with no parent row (shouldn't exist normally) — set to current date as fallback
            migrationBuilder.Sql(@"
                UPDATE TraderTransactionItems SET Date = SYSUTCDATETIME() WHERE Date IS NULL;
            ");

            // 4) Make Date NOT NULL
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "TraderTransactionItems",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TraderTransactionItems_Date",
                table: "TraderTransactionItems",
                column: "Date");

            // 5) Drop the header date column
            migrationBuilder.DropColumn(
                name: "Date",
                table: "TraderTransactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "TraderTransactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Best-effort: copy latest item date back into the header
            migrationBuilder.Sql(@"
                UPDATE t
                SET t.Date = COALESCE((SELECT MAX(i.Date) FROM TraderTransactionItems i WHERE i.TraderTransactionId = t.Id), SYSUTCDATETIME())
                FROM TraderTransactions t;
            ");

            migrationBuilder.DropIndex(
                name: "IX_TraderTransactionItems_Date",
                table: "TraderTransactionItems");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "TraderTransactionItems");
        }
    }
}
