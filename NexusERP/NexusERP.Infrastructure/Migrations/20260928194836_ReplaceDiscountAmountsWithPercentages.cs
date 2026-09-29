using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDiscountAmountsWithPercentages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CartDiscountAmount",
                table: "Receipts",
                newName: "CartDiscountPercentage");

            migrationBuilder.RenameColumn(
                name: "MarketDiscountAmount",
                table: "ReceiptItems",
                newName: "MarketDiscountPercentage");

            migrationBuilder.RenameColumn(
                name: "ManualItemDiscountAmount",
                table: "ReceiptItems",
                newName: "ManualItemDiscountPercentage");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "AllowNegativeInventory",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 28, 19, 48, 35, 951, DateTimeKind.Utc).AddTicks(5448));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "DiscountPolicy",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 28, 19, 48, 35, 951, DateTimeKind.Utc).AddTicks(5450));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "GlobalLowStockThreshold",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 28, 19, 48, 35, 951, DateTimeKind.Utc).AddTicks(5444));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CartDiscountPercentage",
                table: "Receipts",
                newName: "CartDiscountAmount");

            migrationBuilder.RenameColumn(
                name: "MarketDiscountPercentage",
                table: "ReceiptItems",
                newName: "MarketDiscountAmount");

            migrationBuilder.RenameColumn(
                name: "ManualItemDiscountPercentage",
                table: "ReceiptItems",
                newName: "ManualItemDiscountAmount");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "AllowNegativeInventory",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 28, 13, 23, 12, 395, DateTimeKind.Utc).AddTicks(3586));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "DiscountPolicy",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 28, 13, 23, 12, 395, DateTimeKind.Utc).AddTicks(3588));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "GlobalLowStockThreshold",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 28, 13, 23, 12, 395, DateTimeKind.Utc).AddTicks(3583));
        }
    }
}
