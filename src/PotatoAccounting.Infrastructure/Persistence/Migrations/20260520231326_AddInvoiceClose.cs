using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotatoAccounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceClose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "FarmerTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosedBy",
                table: "FarmerTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "FarmerTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "FarmerTransactions");

            migrationBuilder.DropColumn(
                name: "ClosedBy",
                table: "FarmerTransactions");

            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "FarmerTransactions");
        }
    }
}
