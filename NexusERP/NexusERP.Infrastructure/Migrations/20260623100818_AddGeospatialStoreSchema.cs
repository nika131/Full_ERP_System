using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace NexusERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGeospatialStoreSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "InventoryTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    StoreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<Point>(type: "geography", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.StoreId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemAuditLogs_CreatedAt_LogId",
                table: "SystemAuditLogs",
                columns: new[] { "CreatedAt", "LogId" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_CreatedAt_TransactionId",
                table: "InventoryTransactions",
                columns: new[] { "CreatedAt", "TransactionId" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_StoreId",
                table: "InventoryTransactions",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Stores_StoreId",
                table: "InventoryTransactions",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "StoreId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Stores_StoreId",
                table: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_SystemAuditLogs_CreatedAt_LogId",
                table: "SystemAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_CreatedAt_TransactionId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_StoreId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "InventoryTransactions");
        }
    }
}
