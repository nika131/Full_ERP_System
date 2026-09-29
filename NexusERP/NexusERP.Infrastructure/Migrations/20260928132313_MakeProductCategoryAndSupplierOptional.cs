using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeProductCategoryAndSupplierOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "AllowNegativeInventory",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 17, 22, 45, 54, 779, DateTimeKind.Utc).AddTicks(722));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "DiscountPolicy",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 17, 22, 45, 54, 779, DateTimeKind.Utc).AddTicks(724));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "GlobalLowStockThreshold",
                column: "UpdatedAt",
                value: new DateTime(2026, 9, 17, 22, 45, 54, 779, DateTimeKind.Utc).AddTicks(719));
        }
    }
}
