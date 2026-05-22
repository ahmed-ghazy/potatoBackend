using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotatoAccounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create Tenants table
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name",
                unique: true);

            // 2. Insert default tenant — auto-assigned Id = 1 (table is empty)
            migrationBuilder.Sql(
                "INSERT INTO [Tenants] ([Name], [IsActive], [CreatedAt], [CreatedBy]) " +
                "VALUES (N'الشركة الافتراضية', 1, SYSUTCDATETIME(), NULL);");

            // 3. Add TenantId columns. Existing rows are backfilled to the first tenant (Id from previous insert).
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "FarmerTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.Sql(
                "UPDATE [FarmerTransactions] SET [TenantId] = (SELECT TOP 1 [Id] FROM [Tenants] ORDER BY [Id]);");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Expenses",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.Sql(
                "UPDATE [Expenses] SET [TenantId] = (SELECT TOP 1 [Id] FROM [Tenants] ORDER BY [Id]);");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
            // Attach all existing users to default tenant so admin@potato.local can keep logging in.
            migrationBuilder.Sql(
                "UPDATE [AspNetUsers] SET [TenantId] = (SELECT TOP 1 [Id] FROM [Tenants] ORDER BY [Id]) WHERE [TenantId] IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_FarmerTransactions_TenantId",
                table: "FarmerTransactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_TenantId",
                table: "Expenses",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FarmerTransactions_TenantId",
                table: "FarmerTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_TenantId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "FarmerTransactions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
