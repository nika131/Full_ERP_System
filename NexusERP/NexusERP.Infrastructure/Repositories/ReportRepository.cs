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
                Enum.TryParse(row["TransactionType"].ToString(), out TransactionType parsedType);

                transactions.Add(new InventoryTransaction
                {
                    TransactionId = Convert.ToInt32(row["TransactionId"]),
                    ProductId = Convert.ToInt32(row["ProductId"]),
                    ProductName = row["ProductName"] == DBNull.Value ? string.Empty : row["ProductName"].ToString()!,
                    SupplierId = row["SupplierId"] == DBNull.Value ? 0 : Convert.ToInt32(row["SupplierId"]),
                    SupplierName = row["SupplierName"] == DBNull.Value ? string.Empty : row["SupplierName"].ToString()!,
                    TransactionType = parsedType,
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    Amount = Convert.ToDecimal(row["Amount"]),
                    TransactionDate = Convert.ToDateTime(row["TransactionDate"])
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
                Enum.TryParse(row["TransactionType"].ToString(), out TransactionType parsedType);

                transactions.Add(new InventoryTransaction
                {
                    TransactionId = Convert.ToInt32(row["TransactionId"]),
                    ProductId = Convert.ToInt32(row["ProductId"]),
                    ProductName = row["ProductName"] == DBNull.Value ? string.Empty : row["ProductName"].ToString()!,
                    SupplierId = row["SupplierId"] == DBNull.Value ? 0 : Convert.ToInt32(row["SupplierId"]),
                    SupplierName = row["SupplierName"] == DBNull.Value ? string.Empty : row["SupplierName"].ToString()!,
                    TransactionType = parsedType,
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    Amount = Convert.ToDecimal(row["Amount"]),
                    TransactionDate = Convert.ToDateTime(row["TransactionDate"])
                });
            }
            return transactions;
        }
    }
}
