using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquePosPins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PosPinLookupHash",
                table: "Users",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId_PosPinLookupHash",
                table: "Users",
                columns: new[] { "CompanyId", "PosPinLookupHash" },
                unique: true,
                filter: "[IsActive] = 1 AND [PosPinLookupHash] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_CompanyId_PosPinLookupHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PosPinLookupHash",
                table: "Users");
        }
    }
}
