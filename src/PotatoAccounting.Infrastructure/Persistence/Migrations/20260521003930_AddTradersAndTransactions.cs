using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotatoAccounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTradersAndTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Traders",
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
                    table.PrimaryKey("PK_Traders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TraderTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TraderId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraderTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraderTransactions_Traders_TraderId",
                        column: x => x.TraderId,
                        principalTable: "Traders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TraderPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraderTransactionId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TraderPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraderPayments_TraderTransactions_TraderTransactionId",
                        column: x => x.TraderTransactionId,
                        principalTable: "TraderTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TraderTransactionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TraderTransactionId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HasTare = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    HasCommission = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CommissionPerTon = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraderTransactionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraderTransactionItems_TraderTransactions_TraderTransactionId",
                        column: x => x.TraderTransactionId,
                        principalTable: "TraderTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TraderPayments_TenantId",
                table: "TraderPayments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TraderPayments_TraderTransactionId",
                table: "TraderPayments",
                column: "TraderTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Traders_TenantId_Name",
                table: "Traders",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TraderTransactionItems_TraderTransactionId",
                table: "TraderTransactionItems",
                column: "TraderTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TraderTransactions_TenantId",
                table: "TraderTransactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TraderTransactions_TraderId",
                table: "TraderTransactions",
                column: "TraderId");

            migrationBuilder.CreateIndex(
                name: "IX_TraderTransactions_UserId",
                table: "TraderTransactions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TraderPayments");

            migrationBuilder.DropTable(
                name: "TraderTransactionItems");

            migrationBuilder.DropTable(
                name: "TraderTransactions");

            migrationBuilder.DropTable(
                name: "Traders");
        }
    }
}
