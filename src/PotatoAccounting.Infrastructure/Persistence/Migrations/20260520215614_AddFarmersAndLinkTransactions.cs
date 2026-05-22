using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotatoAccounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFarmersAndLinkTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Create Farmers table
            migrationBuilder.CreateTable(
                name: "Farmers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farmers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Farmers_TenantId_Name",
                table: "Farmers",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            // 2) Add FarmerId column as nullable so we can backfill
            migrationBuilder.AddColumn<int>(
                name: "FarmerId",
                table: "FarmerTransactions",
                type: "int",
                nullable: true);

            // 3) Backfill: create one Farmer per unique (TenantId, Name) from existing transactions
            migrationBuilder.Sql(@"
                INSERT INTO Farmers (Name, PhoneNumber, TenantId, CreatedAt, CreatedBy)
                SELECT DISTINCT LTRIM(RTRIM(Name)), NULL, TenantId, SYSUTCDATETIME(), NULL
                FROM FarmerTransactions
                WHERE Name IS NOT NULL AND LTRIM(RTRIM(Name)) <> '';
            ");

            // 4) Link each transaction to its Farmer
            migrationBuilder.Sql(@"
                UPDATE t
                SET t.FarmerId = f.Id
                FROM FarmerTransactions t
                INNER JOIN Farmers f
                    ON f.TenantId = t.TenantId
                   AND f.Name = LTRIM(RTRIM(t.Name));
            ");

            // 5) Drop legacy Name column
            migrationBuilder.DropColumn(
                name: "Name",
                table: "FarmerTransactions");

            // 6) Make FarmerId required
            migrationBuilder.AlterColumn<int>(
                name: "FarmerId",
                table: "FarmerTransactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 7) Index + FK
            migrationBuilder.CreateIndex(
                name: "IX_FarmerTransactions_FarmerId",
                table: "FarmerTransactions",
                column: "FarmerId");

            migrationBuilder.AddForeignKey(
                name: "FK_FarmerTransactions_Farmers_FarmerId",
                table: "FarmerTransactions",
                column: "FarmerId",
                principalTable: "Farmers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore Name column
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "FarmerTransactions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            // Best-effort copy of farmer name back into transactions
            migrationBuilder.Sql(@"
                UPDATE t
                SET t.Name = f.Name
                FROM FarmerTransactions t
                INNER JOIN Farmers f ON f.Id = t.FarmerId;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_FarmerTransactions_Farmers_FarmerId",
                table: "FarmerTransactions");

            migrationBuilder.DropIndex(
                name: "IX_FarmerTransactions_FarmerId",
                table: "FarmerTransactions");

            migrationBuilder.DropColumn(
                name: "FarmerId",
                table: "FarmerTransactions");

            migrationBuilder.DropTable(
                name: "Farmers");
        }
    }
}
