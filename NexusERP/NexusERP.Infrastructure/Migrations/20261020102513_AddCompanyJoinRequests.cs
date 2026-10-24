using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyJoinRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyJoinRequests",
                columns: table => new
                {
                    JoinRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedRoleId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyJoinRequests", x => x.JoinRequestId);
                    table.ForeignKey(
                        name: "FK_CompanyJoinRequests_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyJoinRequests_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyJoinRequests_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyJoinRequests_AccountId_CompanyId",
                table: "CompanyJoinRequests",
                columns: new[] { "AccountId", "CompanyId" },
                unique: true,
                filter: "[Status] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyJoinRequests_CompanyId_Status",
                table: "CompanyJoinRequests",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyJoinRequests_ReviewedByUserId",
                table: "CompanyJoinRequests",
                column: "ReviewedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyJoinRequests");
        }
    }
}
