using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Services
{
    public interface IExcelExportService
    {
        void ExcelTransactions(IEnumerable<InventoryTransaction> data, string filePath, string sheetName = "Transactions");

        byte[] ExcelTransactions(IEnumerable<InventoryTransaction> data, string sheetName = "Transactions");
    }
}
