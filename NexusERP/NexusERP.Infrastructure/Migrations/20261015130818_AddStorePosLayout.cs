using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStorePosLayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "AllowNegativeInventory",
                column: "UpdatedAt",
                value: new DateTime(2026, 10, 15, 13, 8, 17, 787, DateTimeKind.Utc).AddTicks(2204));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "DiscountPolicy",
                column: "UpdatedAt",
                value: new DateTime(2026, 10, 15, 13, 8, 17, 787, DateTimeKind.Utc).AddTicks(2207));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "GlobalLowStockThreshold",
                column: "UpdatedAt",
                value: new DateTime(2026, 10, 15, 13, 8, 17, 787, DateTimeKind.Utc).AddTicks(2201));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "AllowNegativeInventory",
                column: "UpdatedAt",
                value: new DateTime(2026, 10, 9, 13, 52, 27, 644, DateTimeKind.Utc).AddTicks(5638));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "DiscountPolicy",
                column: "UpdatedAt",
                value: new DateTime(2026, 10, 9, 13, 52, 27, 644, DateTimeKind.Utc).AddTicks(5639));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "GlobalLowStockThreshold",
                column: "UpdatedAt",
                value: new DateTime(2026, 10, 9, 13, 52, 27, 644, DateTimeKind.Utc).AddTicks(5633));
        }
    }
}
