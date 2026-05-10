using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.Data;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Product_Inventory_Manager.Product_Inventory_Manager.Services
{
    internal class ExportService
    {
        public void ExportDataTableToExcel(DataTable data, string filePath, string sheetName)
        {
            if (data == null || data.Rows.Count == 0)
                throw new Exception("There is no datato export.");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(data, sheetName);

                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }
        }
    }
}
