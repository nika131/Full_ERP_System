using Product_Inventory_Manager.Data;
using Product_Inventory_Manager.Presenters;
using Product_Inventory_Manager.Product_Inventory_Manager.Views;
using Product_Inventory_Manager.Repositories;
using Product_Inventory_Manager.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Product_Inventory_Manager
{
    public partial class Form1 : Form, IMainView
    {
        private MainPresenter _presenter;
        public Form1()
        {
            InitializeComponent();

            dgvProducts.AutoGenerateColumns = false;
            dgvProducts.AllowUserToAddRows = false;

            dgvProducts.AutoGenerateColumns = false;
            _presenter = new MainPresenter(this, new ProductRepository());
            _presenter.refreshData();
            this.dgvProducts.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvProducts_CellFormatting);

            this.dgvProducts.MouseDown += new MouseEventHandler(this.dgvProducts_MouseDown);
            this.Click += new EventHandler(this.Form1_Click);
        }

        // --- GRID & LABELS ---
        public DataTable gridDataSource { set => dgvProducts.DataSource = value; }
        public string totalItemsText { set => lblTotalItems.Text = value; }
        public string totalValueText { set => lblTotalValue.Text = value; }
        public string totalProfitText { set => lblTotalProfit.Text = value; }
        public string lowStockText { set => lblLowStock.Text = value; }
        public Color lowStockColor { set => lblLowStock.ForeColor = value; }

        // --- PRODUCT METADATA (For saveProduct) ---
        public int productId
        {
            get => int.TryParse(txtId.Text, out int id) ? id : 0;
            set => txtId.Text = value.ToString();
        }
        public string productName { get => txtName.Text; set => txtName.Text = value; }
        public decimal productPrice { get => numPrice.Value; set => numPrice.Value = value; }
        public decimal costPrice { get => numCostPrice.Value; set => numCostPrice.Value = value; }

        // Categories & Suppliers
        private int _initialCatId;
        public int initialCategoryId { get => _initialCatId; set => _initialCatId = value; }
        public int categoryId
        {
            get => cbCategory.SelectedValue != null ? Convert.ToInt32(cbCategory.SelectedValue) : _initialCatId;
            set => cbCategory.SelectedValue = value;
        }

        private int _initialSupId;
        public int initialSupplierId { get => _initialSupId; set => _initialSupId = value; }
        public int supplierId
        {
            get => cbSupplier.SelectedValue != null ? Convert.ToInt32(cbSupplier.SelectedValue) : _initialSupId;
            set => cbSupplier.SelectedValue = value;
        }

        // --- TRANSACTION DATA ---
        public int productQuantity { get => (int)numQuantity.Value; set => numQuantity.Value = value; }

        public int soldQty { get => (int)numSell.Value; set => numSell.Value = value; }
        public string transactionType
        {
            get => cbTransaction.SelectedItem?.ToString();
            set => cbTransaction.SelectedItem = value;
        }



        public void showError(string message) => MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public void showMessage(string message) => MessageBox.Show(message);

        private void txtSearch_TextChanged_1(object sender, EventArgs e)
        {
            _presenter.search(txtSearch.Text);
        }

        public bool confirmDelete(string productName)
        {
            return MessageBox.Show($"Are you sure you want to delete {productName}?",
                "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null)
            {
                int id = (int)dgvProducts.CurrentRow.Cells["ProductId"].Value;
                string name = dgvProducts.CurrentRow.Cells["ProductName"].Value.ToString();
                _presenter.deleteProduct(id, name);
            }
        }


        private void dgvProducts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvProducts.Columns[e.ColumnIndex].Name == "Quantity" && e.Value != null)
            {
                if (int.TryParse(e.Value.ToString(), out int qty))
                {
                    if (qty < 5)
                    {
                        e.CellStyle.BackColor = Color.Salmon;
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.SelectionBackColor = Color.DarkRed;
                    }
                    else
                    {
                        e.CellStyle.BackColor = dgvProducts.DefaultCellStyle.BackColor;
                        e.CellStyle.ForeColor = dgvProducts.DefaultCellStyle.ForeColor;
                    }
                }
            }
        }

        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvProducts.Rows[e.RowIndex];

                if (row.IsNewRow) return;

                txtId.Text = row.Cells["ProductId"].Value.ToString();
                txtName.Text = row.Cells["ProductName"].Value.ToString();
                numPrice.Value = (decimal)row.Cells["ProductPrice"].Value;
                numQuantity.Value = (int)row.Cells["Quantity"].Value;
                numCostPrice.Value = (decimal)row.Cells["CostPrice"].Value;
                cbCategory.SelectedValue = Convert.ToInt32(row.Cells["CategoryId"].Value);
                cbSupplier.SelectedValue = Convert.ToInt32(row.Cells["SupplierId"].Value);
            }
            else
            {
                clearInputFields();
            }
        }

        public void clearInputFields()
        {
            dgvProducts.CurrentCell = null;
            txtId.Text = "";
            txtName.Text = "";
            numPrice.Value = 0;
            numCostPrice.Value = 0;
            numQuantity.Value = 0;
            cbCategory.SelectedIndex = -1;
            cbSupplier.SelectedIndex = -1;
        }

        private void dgvProducts_MouseDown(object sender, MouseEventArgs e)
        {
            DataGridView.HitTestInfo hit = dgvProducts.HitTest(e.X, e.Y);

            if (hit.Type == DataGridViewHitTestType.None)
            {
                dgvProducts.ClearSelection();
                clearInputFields();
            }
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            dgvProducts.ClearSelection();
            clearInputFields();
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            string action = cbTransaction.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(action))
            {
                showMessage("Please select an action from the dropdown first.");
                return;
            }

            if (action == "New Product")
            {
                txtId.Text = "0";
                _presenter.saveProduct();
                _presenter.refreshData();
                clearInputFields(); 
            }
            else if (action == "Update Details")
            {
                _presenter.saveProduct();
                _presenter.refreshData();
            }
            else if (action == "IN" || action == "OUT" || action == "ADJ")
            {
                _presenter.makeTransaction();
                _presenter.refreshData();
                clearInputFields();
            }
        }

        private void cbTransaction_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbTransaction.SelectedItem?.ToString() == "New Product")
            {
                clearInputFields();
            }
        }

        public void loadCategories(DataTable categories)
        {
            cbCategory.DataSource = categories;
            cbCategory.DisplayMember = "Name";
            cbCategory.ValueMember = "CategoryId";
        }

        public void loadSuppliers(DataTable suppliers)
        {
            cbSupplier.DataSource = suppliers;
            cbSupplier.DisplayMember = "CompanyName";
            cbSupplier.ValueMember = "SupplierId";
        }

    }
}
