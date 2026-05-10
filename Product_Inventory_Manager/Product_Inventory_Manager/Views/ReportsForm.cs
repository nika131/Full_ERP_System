using Product_Inventory_Manager.Product_Inventory_Manager.Models;
using Product_Inventory_Manager.Product_Inventory_Manager.Presenters;
using Product_Inventory_Manager.Product_Inventory_Manager.Repositories;
using Product_Inventory_Manager.Product_Inventory_Manager.Services;
using Product_Inventory_Manager.Product_Inventory_Manager.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Product_Inventory_Manager.Product_Inventory_Manager.Views
{
    public partial class ReportsForm : Form, IReportView
    {
        private ReportPresenter _presenter;
        public ReportsForm()
        {
            InitializeComponent();

            dgvReports.AutoGenerateColumns = false;
            dgvReports.AllowUserToAddRows = false;

            _presenter = new ReportPresenter(new ReportRepository(), this);
            _presenter.RefreshData();
        }

        public DataTable GridDataSource { set => dgvReports.DataSource = value; }

        public void ShowMessage(string message) => MessageBox.Show(message);


        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd =  new SaveFileDialog())
            {
                sfd.Filter = "Excel Workbook|*.xlsx";
                sfd.Title = "Save Inventory Report";
                sfd.FileName = "Inventory_Report_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    DataTable dt = (DataTable)dgvReports.DataSource;
                    _presenter.exportReport(sfd.FileName, dt);
                }
            }
        }

        private void dgvReports_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtId.Text = dgvReports.Rows[e.RowIndex].Cells["TransactionId"].Value.ToString();
            }
            else
            {
                txtId.Text = "";
            }
        }

        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            if (dgvReports.CurrentRow == null)
            {
                ShowMessage("Please select a transaction from the list first.");
                return;
            }

            var row = dgvReports.CurrentRow;
            var model = new InvoiceModel
            {
                TransactionId = Convert.ToInt32(row.Cells["TransactionId"].Value),
                ProductName = row.Cells["ProductName"].Value == DBNull.Value ? "Unknown" : row.Cells["ProductName"].Value.ToString(),
                SupplierName = row.Cells["SupplierName"].Value == DBNull.Value ? "N/A" : row.Cells["SupplierName"].Value.ToString(),
                TransactionType = row.Cells["TransactionType"].Value == DBNull.Value ? "Unknown" : row.Cells["TransactionType"].Value.ToString(),
                Quantity = row.Cells["Quantity"].Value == DBNull.Value ? 0 : Convert.ToInt32(row.Cells["Quantity"].Value),
                Amount = row.Cells["Amount"].Value == DBNull.Value ? 0m : Convert.ToDecimal(row.Cells["Amount"].Value),
                Date = row.Cells["TransactionDate"].Value == DBNull.Value ? DateTime.Now : Convert.ToDateTime(row.Cells["TransactionDate"].Value)
            };

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF Document|*.pdf";
                sfd.Title = "Save Transaction Invoice";
                sfd.FileName = $"Invoice_{model.TransactionId}_{DateTime.Now:yyyyMMdd}.pdf";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        new PdfExportService().GenerateInvoice(model, sfd.FileName);
                        ShowMessage("PDF Invoice saved successfully.");
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("Error generating PDF: " + ex.Message);
                    }
                }
            }
        }
    }
}
