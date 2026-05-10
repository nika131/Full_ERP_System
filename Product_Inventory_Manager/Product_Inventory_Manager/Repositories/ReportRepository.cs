using Product_Inventory_Manager.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product_Inventory_Manager.Product_Inventory_Manager.Repositories
{
    internal class ReportRepository : IReportRepository
    {
        public DataTable GetAll() => DatabaseHelper.ExecuteStoredProcedure("sp_GetAllTransactions");
        public DataTable Search(string Keyword) =>
            DatabaseHelper.ExecuteStoredProcedure("sp_SearchTransactions", new Dictionary<string, object> { { "@Keyword", Keyword } });
    }
}
