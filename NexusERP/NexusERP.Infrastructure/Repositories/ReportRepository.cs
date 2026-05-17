using Microsoft.Data.SqlClient;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        public IEnumerable<InventoryTransaction> GetAll()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAllTransactions");
            var transactions = new List<InventoryTransaction>();

            foreach (DataRow row in dt.Rows)
            {
                Enum.TryParse(row["TransactionType"].ToString(), out TransactionAction parsedType);

                transactions.Add(new InventoryTransaction
                {
                    TransactionId = row["TransactionId"] == DBNull.Value ? 0 : Convert.ToInt32(row["TransactionId"]),
                    ProductId = row["ProductId"] == DBNull.Value ? 0 : Convert.ToInt32(row["ProductId"]),
                    SupplierId = row["SupplierId"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["SupplierId"]),
                    UserId = row["UserId"] == DBNull.Value ? 0 : Convert.ToInt32(row["UserId"]),
                    ProductName = row["ProductName"] == DBNull.Value ? string.Empty : row["ProductName"].ToString()!,
                    SupplierName = row["SupplierName"] == DBNull.Value ? string.Empty : row["SupplierName"].ToString()!,
                    TransactionType = parsedType,
                    Quantity = row["Quantity"] == DBNull.Value ? 0 : Convert.ToInt32(row["Quantity"]),
                    UnitPrice = row["UnitPrice"] == DBNull.Value ? 0m : Convert.ToDecimal(row["UnitPrice"]),
                    TotalAmount = row["TotalAmount"] == DBNull.Value ? 0m : Convert.ToDecimal(row["TotalAmount"]),
                    Profit = row["Profit"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Profit"]),
                    CreatedAt = row["CreatedAt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["CreatedAt"]),
                });
            }
            return transactions;
        }
        public IEnumerable<InventoryTransaction> Search(string Keyword)
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_SearchTransactions", new Dictionary<string, object> { { "@Keyword", Keyword } });
            var transactions = new List<InventoryTransaction>();

            foreach (DataRow row in dt.Rows)
            {
                Enum.TryParse(row["TransactionType"].ToString(), out TransactionAction parsedType);

                transactions.Add(new InventoryTransaction
                {
                    TransactionId = row["TransactionId"] == DBNull.Value ? 0 : Convert.ToInt32(row["TransactionId"]),
                    ProductId = row["ProductId"] == DBNull.Value ? 0 : Convert.ToInt32(row["ProductId"]),
                    SupplierId = row["SupplierId"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["SupplierId"]),
                    UserId = row["UserId"] == DBNull.Value ? 0 : Convert.ToInt32(row["UserId"]), // MAPPED!
                    ProductName = row["ProductName"] == DBNull.Value ? string.Empty : row["ProductName"].ToString()!,
                    SupplierName = row["SupplierName"] == DBNull.Value ? string.Empty : row["SupplierName"].ToString()!,
                    TransactionType = parsedType,
                    Quantity = row["Quantity"] == DBNull.Value ? 0 : Convert.ToInt32(row["Quantity"]),
                    UnitPrice = row["UnitPrice"] == DBNull.Value ? 0m : Convert.ToDecimal(row["UnitPrice"]),
                    TotalAmount = row["TotalAmount"] == DBNull.Value ? 0m : Convert.ToDecimal(row["TotalAmount"]),
                    Profit = row["Profit"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Profit"]),
                    CreatedAt = row["CreatedAt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["CreatedAt"]),
                });
            }
            return transactions;
        }
    }
}
