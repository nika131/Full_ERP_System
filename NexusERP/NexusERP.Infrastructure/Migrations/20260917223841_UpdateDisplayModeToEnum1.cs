using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDisplayModeToEnum1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "AllowNegativeInventory",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 17, 22, 38, 41, 313, DateTimeKind.Utc).AddTicks(8895));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "DiscountPolicy",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 17, 22, 38, 41, 313, DateTimeKind.Utc).AddTicks(8897));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "GlobalLowStockThreshold",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 17, 22, 38, 41, 313, DateTimeKind.Utc).AddTicks(8893));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "AllowNegativeInventory",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 17, 22, 21, 39, 744, DateTimeKind.Utc).AddTicks(5414));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "DiscountPolicy",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 17, 22, 21, 39, 744, DateTimeKind.Utc).AddTicks(5415));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "GlobalLowStockThreshold",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 17, 22, 21, 39, 744, DateTimeKind.Utc).AddTicks(5411));
        }
    }
}
