using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.Data;
using DocumentFormat.OpenXml.Spreadsheet;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Entities;

namespace NexusERP.Infrastructure.Services
{
    public class ExcelExportService : IExcelExportService
    {
        public void ExcelTransactions(IEnumerable<InventoryTransaction> data, string filePath, string sheetName)
        {
            if (data == null || !data.Any())
                throw new Exception("There is no datato export.");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(sheetName);
                worksheet.Cell(1, 1).InsertTable(data);

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }
    }
}
