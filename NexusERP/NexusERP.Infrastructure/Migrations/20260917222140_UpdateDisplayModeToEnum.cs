using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDisplayModeToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayMode",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayMode",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "AllowNegativeInventory",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 17, 21, 48, 37, 589, DateTimeKind.Utc).AddTicks(7685));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "DiscountPolicy",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 17, 21, 48, 37, 589, DateTimeKind.Utc).AddTicks(7687));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "GlobalLowStockThreshold",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 17, 21, 48, 37, 589, DateTimeKind.Utc).AddTicks(7682));
        }
    }
}
