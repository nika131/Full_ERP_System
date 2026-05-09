using Product_Inventory_Manager.Product_Inventory_Manager.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Product_Inventory_Manager.Presenters;
using Product_Inventory_Manager.Product_Inventory_Manager.Presenters;
using Product_Inventory_Manager.Product_Inventory_Manager.Repositories;

namespace Product_Inventory_Manager
{
    public partial class SupplierForm : Form, ISupplierView
    {
        private SupplierPresenter _presenter;
        public SupplierForm()
        {
            InitializeComponent();
            dgvSuppliers.AutoGenerateColumns = false;
            _presenter = new SupplierPresenter(this, new SupplierRepository());
            _presenter.refreshData();
        }

        public DataTable SupplierGridDataSource { get => (DataTable)dgvSuppliers.DataSource; set => dgvSuppliers.DataSource = value; }
        public int supplierId => dgvSuppliers.CurrentRow != null ? (int)dgvSuppliers.CurrentRow.Cells["SupplierID"].Value : 0;
        public string companyName => tbCompName.Text;
        public string contactName => tbContactName.Text;
        public string phone => tbPhone.Text;
        public string email => tbEmail.Text;
    
        public void showMessage(string message)
        {
            MessageBox.Show(message);
        }

        private void tbSearchSupplier_TextChanged(object sender, EventArgs e)
        {
            _presenter.searchSuppliers(tbSearchSupplier.Text);
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            _presenter.saveSupplier();
            clearInputFields();
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            dgvSuppliers.CurrentCell = null;
            _presenter.saveSupplier();
            clearInputFields();
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            _presenter.deleteSupplier(supplierId);
            clearInputFields();
        }

        private void dgvSuppliers_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSuppliers.Rows[e.RowIndex];

                tbCompName.Text = row.Cells["CompanyName"].Value?.ToString();
                tbContactName.Text = row.Cells["ContactName"].Value?.ToString();
                tbPhone.Text = row.Cells["Phone"].Value?.ToString();
                tbEmail.Text = row.Cells["Email"].Value?.ToString();
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
            tbContactName.Text = "";
            tbPhone.Text = "";
            tbEmail.Text = "";
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

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void tbCompName_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
