using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotatoAccounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Create the Seasons table first so we can reference it during backfill.
            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_TenantId_Name",
                table: "Seasons",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_TenantId_Default",
                table: "Seasons",
                column: "TenantId",
                unique: true,
                filter: "[IsDefault] = 1");

            // 2) Add SeasonId columns to the four entities as nullable so existing rows survive.
            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "TraderTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "Refrigerators",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "FarmerTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "Expenses",
                type: "int",
                nullable: true);

            // 3) Backfill: create one default season per existing tenant, then point existing rows at it.
            migrationBuilder.Sql(@"
                INSERT INTO Seasons (Name, Notes, IsDefault, TenantId, CreatedAt, CreatedBy)
                SELECT N'موسم افتراضى', NULL, 1, t.Id, SYSUTCDATETIME(), NULL
                FROM Tenants t
                WHERE NOT EXISTS (
                    SELECT 1 FROM Seasons s WHERE s.TenantId = t.Id AND s.IsDefault = 1
                );
            ");

            migrationBuilder.Sql(@"
                UPDATE ft
                SET ft.SeasonId = s.Id
                FROM FarmerTransactions ft
                INNER JOIN Seasons s ON s.TenantId = ft.TenantId AND s.IsDefault = 1
                WHERE ft.SeasonId IS NULL;

                UPDATE tt
                SET tt.SeasonId = s.Id
                FROM TraderTransactions tt
                INNER JOIN Seasons s ON s.TenantId = tt.TenantId AND s.IsDefault = 1
                WHERE tt.SeasonId IS NULL;

                UPDATE e
                SET e.SeasonId = s.Id
                FROM Expenses e
                INNER JOIN Seasons s ON s.TenantId = e.TenantId AND s.IsDefault = 1
                WHERE e.SeasonId IS NULL;

                UPDATE r
                SET r.SeasonId = s.Id
                FROM Refrigerators r
                INNER JOIN Seasons s ON s.TenantId = r.TenantId AND s.IsDefault = 1
                WHERE r.SeasonId IS NULL;
            ");

            // 4) Now that every row has a SeasonId, tighten the columns to NOT NULL.
            migrationBuilder.AlterColumn<int>(
                name: "SeasonId",
                table: "TraderTransactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SeasonId",
                table: "Refrigerators",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SeasonId",
                table: "FarmerTransactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SeasonId",
                table: "Expenses",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 5) Indexes + foreign keys.
            migrationBuilder.CreateIndex(
                name: "IX_TraderTransactions_SeasonId",
                table: "TraderTransactions",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Refrigerators_SeasonId",
                table: "Refrigerators",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmerTransactions_SeasonId",
                table: "FarmerTransactions",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_SeasonId",
                table: "Expenses",
                column: "SeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Seasons_SeasonId",
                table: "Expenses",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FarmerTransactions_Seasons_SeasonId",
                table: "FarmerTransactions",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Refrigerators_Seasons_SeasonId",
                table: "Refrigerators",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TraderTransactions_Seasons_SeasonId",
                table: "TraderTransactions",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Seasons_SeasonId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_FarmerTransactions_Seasons_SeasonId",
                table: "FarmerTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Refrigerators_Seasons_SeasonId",
                table: "Refrigerators");

            migrationBuilder.DropForeignKey(
                name: "FK_TraderTransactions_Seasons_SeasonId",
                table: "TraderTransactions");

            migrationBuilder.DropIndex(
                name: "IX_TraderTransactions_SeasonId",
                table: "TraderTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Refrigerators_SeasonId",
                table: "Refrigerators");

            migrationBuilder.DropIndex(
                name: "IX_FarmerTransactions_SeasonId",
                table: "FarmerTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_SeasonId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "TraderTransactions");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "Refrigerators");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "FarmerTransactions");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "Expenses");

            migrationBuilder.DropTable(
                name: "Seasons");
        }
    }
}
