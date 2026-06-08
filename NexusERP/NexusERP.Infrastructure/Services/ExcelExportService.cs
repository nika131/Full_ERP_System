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
using NexusERP.Domain.Exceptions;

namespace NexusERP.Infrastructure.Services
{
    public class ExcelExportService : IExcelExportService
    {
        public byte[] ExcelTransactions(IEnumerable<InventoryTransaction> data, string sheetName = "Transactions")
        {
            if (data == null || !data.Any())
                throw new AppException("There is no datato export.");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(sheetName);
                worksheet.Cell(1, 1).InsertTable(data);
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public void ExcelTransactions(IEnumerable<InventoryTransaction> data, string filePath, string sheetName = "Transactions")
        {
            byte[] fileBytes = ExcelTransactions(data, sheetName);

            File.WriteAllBytes(filePath, fileBytes);
        }
    }
}
