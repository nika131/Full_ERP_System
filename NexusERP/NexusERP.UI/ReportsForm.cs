using NexusERP.Application.Interfaces.Services;
using NexusERP.Application.Interfaces.Views;
using NexusERP.Application.Presenters;
using NexusERP.Domain.Entities;
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

namespace NexusERP.UI
{
    public partial class ReportsForm : Form, IReportView
    {
        private readonly ReportPresenter _presenter;
        public ReportsForm(ReportPresenter presenter)
        {
            InitializeComponent();

            dgvReports.AutoGenerateColumns = false;
            dgvReports.AllowUserToAddRows = false;

            _presenter = presenter;
            _presenter.SetView(this);
            _presenter.RefreshData();
        }

        public IEnumerable<InventoryTransaction> GridDataSource { set => dgvReports.DataSource = value.ToList(); }

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
                    var data = (IEnumerable<InventoryTransaction>)dgvReports.DataSource;
                    _presenter.ExportExcel(sfd.FileName, data);
                }
            }
        }

        private void dgvReports_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var transaction = (InventoryTransaction)dgvReports.Rows[e.RowIndex].DataBoundItem;
                txtId.Text = transaction.TransactionId.ToString();
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

            var selectedTransaction = (InventoryTransaction)dgvReports.CurrentRow.DataBoundItem;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF Document|*.pdf";
                sfd.Title = "Save Transaction Invoice";
                sfd.FileName = $"Invoice_{selectedTransaction.TransactionId}_{DateTime.Now:yyyyMMdd}.pdf";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    _presenter.ExportPdf(sfd.FileName, selectedTransaction);
                }
            }
        }
    }
}
