using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NexusERP.Infrastructure.Database;

#nullable disable

namespace NexusERP.Infrastructure.Migrations
{
    public partial class ReceiptLineFinancialAllocations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(name: "FinalAmount", table: "ReceiptItems",
                type: "decimal(18,2)", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "AllocatedVatAmount", table: "ReceiptItems",
                type: "decimal(18,2)", nullable: false, defaultValue: 0m);
            // Backfill existing sales exclusively from persisted receipt/line values.
            migrationBuilder.Sql(@";WITH Base AS (
                 SELECT i.ItemId, i.ReceiptId, i.LineTotal, i.VatRate, r.FinalTotal, r.TotalVatAmount,
                 SUM(i.LineTotal) OVER(PARTITION BY i.ReceiptId) AS TotalLines,
                 ROW_NUMBER() OVER(PARTITION BY i.ReceiptId ORDER BY i.ItemId DESC) AS ReverseRow
                 FROM dbo.ReceiptItems i JOIN dbo.Receipts r ON r.ReceiptId=i.ReceiptId
                ), PreAmount AS (
                 SELECT *, CAST(CASE WHEN ReverseRow=1 THEN 0
                   WHEN TotalLines>0 THEN ROUND(FinalTotal*LineTotal/TotalLines,2)
                   ELSE 0 END AS decimal(18,2)) AS RoundedAmount FROM Base
                ), Amounts AS (
                 SELECT *,CAST(CASE WHEN ReverseRow=1 THEN FinalTotal-SUM(RoundedAmount) OVER(PARTITION BY ReceiptId)
                   ELSE RoundedAmount END AS decimal(18,2)) AS Amount FROM PreAmount
                ), Weights AS (
                 SELECT *,CAST(CASE WHEN VatRate>0 THEN Amount*VatRate/(100+VatRate) ELSE 0 END AS decimal(28,8)) AS Weight FROM Amounts
                ), Ranked AS (
                 SELECT *,SUM(Weight) OVER(PARTITION BY ReceiptId) AS TotalWeight,
                   ROW_NUMBER() OVER(PARTITION BY ReceiptId ORDER BY CASE WHEN Weight>0 THEN 0 ELSE 1 END,ItemId DESC) AS VatRemainderRow
                 FROM Weights
                ), PreVat AS (
                 SELECT *,CAST(CASE WHEN VatRemainderRow=1 THEN 0
                   WHEN TotalWeight>0 THEN ROUND(TotalVatAmount*Weight/TotalWeight,2)
                   ELSE 0 END AS decimal(18,2)) AS RoundedVat FROM Ranked
                ), Allocated AS (
                 SELECT ItemId,Amount, CAST(CASE WHEN VatRemainderRow=1
                   THEN TotalVatAmount-SUM(RoundedVat) OVER(PARTITION BY ReceiptId)
                   ELSE RoundedVat END AS decimal(18,2)) AS VatAmount FROM PreVat
                )
                UPDATE i SET FinalAmount=a.Amount,AllocatedVatAmount=a.VatAmount
                FROM dbo.ReceiptItems i JOIN Allocated a ON a.ItemId=i.ItemId;"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FinalAmount", table: "ReceiptItems");
            migrationBuilder.DropColumn(name: "AllocatedVatAmount", table: "ReceiptItems");
        }
    }
}
