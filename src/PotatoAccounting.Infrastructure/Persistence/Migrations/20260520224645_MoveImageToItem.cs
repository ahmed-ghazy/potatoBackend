using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotatoAccounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveImageToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add ImagePath to items
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "FarmerTransactionItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // 2) Backfill: copy each invoice's image to the FIRST item of that invoice
            migrationBuilder.Sql(@"
                WITH FirstItems AS (
                    SELECT FarmerTransactionId, Id,
                           ROW_NUMBER() OVER (PARTITION BY FarmerTransactionId ORDER BY Id) AS rn
                    FROM FarmerTransactionItems
                )
                UPDATE i
                SET i.ImagePath = t.ImagePath
                FROM FarmerTransactionItems i
                INNER JOIN FirstItems fi ON fi.Id = i.Id AND fi.rn = 1
                INNER JOIN FarmerTransactions t ON t.Id = i.FarmerTransactionId
                WHERE t.ImagePath IS NOT NULL AND t.ImagePath <> '';
            ");

            // 3) Drop the legacy column
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "FarmerTransactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "FarmerTransactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // Best-effort: copy the first item's image back into the invoice header
            migrationBuilder.Sql(@"
                WITH FirstItems AS (
                    SELECT FarmerTransactionId, Id, ImagePath,
                           ROW_NUMBER() OVER (PARTITION BY FarmerTransactionId ORDER BY Id) AS rn
                    FROM FarmerTransactionItems
                )
                UPDATE t
                SET t.ImagePath = fi.ImagePath
                FROM FarmerTransactions t
                INNER JOIN FirstItems fi ON fi.FarmerTransactionId = t.Id AND fi.rn = 1
                WHERE fi.ImagePath IS NOT NULL AND fi.ImagePath <> '';
            ");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "FarmerTransactionItems");
        }
    }
}
