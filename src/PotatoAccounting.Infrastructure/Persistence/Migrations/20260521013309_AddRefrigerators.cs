using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotatoAccounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefrigerators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Refrigerators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refrigerators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefrigeratorExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefrigeratorId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefrigeratorExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefrigeratorExpenses_Refrigerators_RefrigeratorId",
                        column: x => x.RefrigeratorId,
                        principalTable: "Refrigerators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefrigeratorItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefrigeratorId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SackCount = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefrigeratorItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefrigeratorItems_Refrigerators_RefrigeratorId",
                        column: x => x.RefrigeratorId,
                        principalTable: "Refrigerators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefrigeratorExpenses_Date",
                table: "RefrigeratorExpenses",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_RefrigeratorExpenses_RefrigeratorId",
                table: "RefrigeratorExpenses",
                column: "RefrigeratorId");

            migrationBuilder.CreateIndex(
                name: "IX_RefrigeratorExpenses_TenantId",
                table: "RefrigeratorExpenses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RefrigeratorItems_Date",
                table: "RefrigeratorItems",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_RefrigeratorItems_RefrigeratorId",
                table: "RefrigeratorItems",
                column: "RefrigeratorId");

            migrationBuilder.CreateIndex(
                name: "IX_RefrigeratorItems_TenantId",
                table: "RefrigeratorItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Refrigerators_TenantId_Name",
                table: "Refrigerators",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefrigeratorExpenses");

            migrationBuilder.DropTable(
                name: "RefrigeratorItems");

            migrationBuilder.DropTable(
                name: "Refrigerators");
        }
    }
}
