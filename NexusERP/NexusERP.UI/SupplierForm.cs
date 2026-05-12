using NexusERP.Application.Interfaces.Views;
using NexusERP.Application.Presenters;
using NexusERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NexusERP.UI
{
    public partial class SupplierForm : Form, ISupplierView
    {
        private SupplierPresenter _presenter;
        public SupplierForm(SupplierPresenter presenter)
        {
            InitializeComponent();
            dgvSuppliers.AutoGenerateColumns = false;

            _presenter = presenter;
            _presenter.SetView(this);
            _presenter.RefreshData();
        }

        public IEnumerable<Supplier> SupplierGridDataSource { set => dgvSuppliers.DataSource = value.ToList(); }
        public int SupplierId => dgvSuppliers.CurrentRow != null ? (int)dgvSuppliers.CurrentRow.Cells["colSupplierID"].Value : 0;
        public string ViewCompanyName => tbCompName.Text;
        public string ContactName => tbcolContactName.Text;
        public string Phone => tbcolPhone.Text;
        public string Email => tbcolEmail.Text;
    
        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        private void tbSearchSupplier_TextChanged(object sender, EventArgs e)
        {
            _presenter.SearchSuppliers(tbSearchSupplier.Text);
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            _presenter.SaveSupplier();
            clearInputFields();
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            dgvSuppliers.CurrentCell = null;
            _presenter.SaveSupplier();
            clearInputFields();
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            _presenter.DeleteSupplier(SupplierId);
            clearInputFields();
        }

        private void dgvSuppliers_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSuppliers.Rows[e.RowIndex];

                tbCompName.Text = row.Cells["colCompanyName"].Value?.ToString();
                tbcolContactName.Text = row.Cells["colContactName"].Value?.ToString();
                tbcolPhone.Text = row.Cells["colPhone"].Value?.ToString();
                tbcolEmail.Text = row.Cells["colEmail"].Value?.ToString();
            }
            else
            {
                clearInputFields();
            }
        }

        public void clearInputFields()
        {
            dgvSuppliers.CurrentCell = null;
            tbCompName.Text = "";
            tbcolContactName.Text = "";
            tbcolPhone.Text = "";
            tbcolEmail.Text = "";
        }

        private void dgvSuppliers_MouseDown(object sender, MouseEventArgs e)
        {
            DataGridView.HitTestInfo hit = dgvSuppliers.HitTest(e.X, e.Y);

            if (hit.Type == DataGridViewHitTestType.None)
            {
                dgvSuppliers.ClearSelection();
                clearInputFields();
            }
        }
    }
}
