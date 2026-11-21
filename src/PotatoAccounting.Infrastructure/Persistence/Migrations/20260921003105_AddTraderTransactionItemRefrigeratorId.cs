using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotatoAccounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTraderTransactionItemRefrigeratorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RefrigeratorId",
                table: "TraderTransactionItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TraderTransactionItems_RefrigeratorId",
                table: "TraderTransactionItems",
                column: "RefrigeratorId");

            migrationBuilder.AddForeignKey(
                name: "FK_TraderTransactionItems_Refrigerators_RefrigeratorId",
                table: "TraderTransactionItems",
                column: "RefrigeratorId",
                principalTable: "Refrigerators",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TraderTransactionItems_Refrigerators_RefrigeratorId",
                table: "TraderTransactionItems");

            migrationBuilder.DropIndex(
                name: "IX_TraderTransactionItems_RefrigeratorId",
                table: "TraderTransactionItems");

            migrationBuilder.DropColumn(
                name: "RefrigeratorId",
                table: "TraderTransactionItems");
        }
    }
}
