using Product_Inventory_Manager.Product_Inventory_Manager.Presenters;
using Product_Inventory_Manager.Product_Inventory_Manager.Repositories;
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
            string folderPath = Path.Combine(Application.StartupPath, "Reports");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

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
    }
}
