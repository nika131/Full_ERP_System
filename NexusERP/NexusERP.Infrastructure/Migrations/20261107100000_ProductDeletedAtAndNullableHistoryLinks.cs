using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NexusERP.Infrastructure.Database;

#nullable disable

namespace NexusERP.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261107100000_ProductDeletedAtAndNullableHistoryLinks")]
    public partial class ProductDeletedAtAndNullableHistoryLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(name: "DeletedAt", table: "Products",
                type: "datetime2", nullable: true);

            migrationBuilder.DropForeignKey(name: "FK_InventoryTransactions_Products_ProductId", table: "InventoryTransactions");
            migrationBuilder.DropForeignKey(name: "FK_ReceiptItems_Products_ProductId", table: "ReceiptItems");

            migrationBuilder.AlterColumn<int>(name: "ProductId", table: "InventoryTransactions",
                type: "int", nullable: true, oldClrType: typeof(int), oldType: "int");
            migrationBuilder.AlterColumn<int>(name: "ProductId", table: "ReceiptItems",
                type: "int", nullable: true, oldClrType: typeof(int), oldType: "int");

            migrationBuilder.AddForeignKey(name: "FK_InventoryTransactions_Products_ProductId",
                table: "InventoryTransactions", column: "ProductId", principalTable: "Products",
                principalColumn: "ProductId", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_ReceiptItems_Products_ProductId",
                table: "ReceiptItems", column: "ProductId", principalTable: "Products",
                principalColumn: "ProductId", onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restoring required FKs is impossible while there are purged product references.
            // Fail clearly rather than inventing historical ProductId values.
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM dbo.ReceiptItems WHERE ProductId IS NULL)
                   OR EXISTS (SELECT 1 FROM dbo.InventoryTransactions WHERE ProductId IS NULL)
                THROW 50001, 'Cannot roll back product nullable references after catalog purging.', 1;
            ");

            migrationBuilder.DropForeignKey(name: "FK_InventoryTransactions_Products_ProductId", table: "InventoryTransactions");
            migrationBuilder.DropForeignKey(name: "FK_ReceiptItems_Products_ProductId", table: "ReceiptItems");
            migrationBuilder.AlterColumn<int>(name: "ProductId", table: "InventoryTransactions",
                type: "int", nullable: false, oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<int>(name: "ProductId", table: "ReceiptItems",
                type: "int", nullable: false, oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AddForeignKey(name: "FK_InventoryTransactions_Products_ProductId",
                table: "InventoryTransactions", column: "ProductId", principalTable: "Products",
                principalColumn: "ProductId", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_ReceiptItems_Products_ProductId",
                table: "ReceiptItems", column: "ProductId", principalTable: "Products",
                principalColumn: "ProductId", onDelete: ReferentialAction.Restrict);
            migrationBuilder.DropColumn(name: "DeletedAt", table: "Products");
        }
    }
}
