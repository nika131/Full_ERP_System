using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OneOpenShiftPerStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Shifts_CompanyId_StoreId",
                table: "Shifts",
                columns: new[] { "CompanyId", "StoreId" },
                unique: true,
                filter: "[Status] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shifts_CompanyId_StoreId",
                table: "Shifts");
        }
    }
}
