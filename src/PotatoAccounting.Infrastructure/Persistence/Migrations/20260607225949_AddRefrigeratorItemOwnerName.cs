using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotatoAccounting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefrigeratorItemOwnerName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "RefrigeratorItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            // One-time backfill: existing items used the Description field
            // as an informal owner label. Copy it into the new OwnerName so the
            // owner summary is immediately useful. The user can edit any rows
            // where Description was actually a description, not an owner.
            migrationBuilder.Sql(@"
                UPDATE RefrigeratorItems
                SET OwnerName = LEFT(LTRIM(RTRIM(Description)), 200)
                WHERE OwnerName IS NULL
                  AND Description IS NOT NULL
                  AND LEN(LTRIM(RTRIM(Description))) > 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "RefrigeratorItems");
        }
    }
}
