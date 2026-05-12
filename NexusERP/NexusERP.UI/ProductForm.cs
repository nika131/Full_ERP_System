using System;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NexusERP.Application.Interfaces.Views;
using NexusERP.Application.Presenters;
using NexusERP.Domain.Entities;

namespace NexusERP.UI
{
    public partial class ProductForm : Form, IProductView
    {
        private readonly ProductPresenter _presenter;
        public ProductForm(ProductPresenter presenter)
        {
            InitializeComponent();

            dgvProducts.AutoGenerateColumns = false;
            dgvProducts.AllowUserToAddRows = false;

            _presenter = presenter;
            _presenter.SetView(this);
            _presenter.RefreshData();

            this.dgvProducts.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvProducts_CellFormatting);
            this.dgvProducts.MouseDown += new MouseEventHandler(this.dgvProducts_MouseDown);
            this.Click += new EventHandler(this.Form1_Click);
        }

        public IEnumerable<Product> GridDataSource
        {
            set => dgvProducts.DataSource = value.ToList();
        }

        // --- GRID & LABELS ---
        public string TotalItemsText { set => lblTotalItems.Text = value; }
        public string TotalValueText { set => lblTotalValue.Text = value; }
        public string TotalProfitText { set => lblTotalProfit.Text = value; }
        public string LowStockText { set => lblLowStock.Text = value; }
        public Color LowStockColor { set => lblLowStock.ForeColor = value; }

        // --- PRODUCT METADATA (For saveProduct) ---
        public int ProductId
        {
            get => int.TryParse(txtId.Text, out int id) ? id : 0;
            set => txtId.Text = value.ToString();
        }
        public string ViewProductName { get => txtName.Text; set => txtName.Text = value; }
        public decimal ProductPrice { get => numPrice.Value; set => numPrice.Value = value; }
        public decimal CostPrice { get => numcolCostPrice.Value; set => numcolCostPrice.Value = value; }

        // Categories & Suppliers
        private int _initialCatId;
        public int InitialCategoryId { get => _initialCatId; set => _initialCatId = value; }
        public int CategoryId
        {
            get => cbCategory.SelectedValue != null ? Convert.ToInt32(cbCategory.SelectedValue) : _initialCatId;
            set => cbCategory.SelectedValue = value;
        }

        private int _initialSupId;
        public int InitialSupplierId { get => _initialSupId; set => _initialSupId = value; }
        public int SupplierId
        {
            get => cbSupplier.SelectedValue != null ? Convert.ToInt32(cbSupplier.SelectedValue) : _initialSupId;
            set => cbSupplier.SelectedValue = value;
        }

        // --- TRANSACTION DATA ---
        public int ProductQuantity { get => (int)numcolQuantity.Value; set => numcolQuantity.Value = value; }

        public int SoldQty { get => (int)numSell.Value; set => numSell.Value = value; }
        public string TransactionType
        {
            get => cbTransaction.SelectedItem?.ToString();
            set => cbTransaction.SelectedItem = value;
        }



        public void ShowError(string message) => MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public void ShowMessage(string message) => MessageBox.Show(message);

        private void txtSearch_TextChanged_1(object sender, EventArgs e)
        {
            _presenter.Search(txtSearch.Text);
        }

        public bool ConfirmDelete(string productName)
        {
            return MessageBox.Show($"Are you sure you want to delete {productName}?",
                "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null)
            {
                int id = (int)dgvProducts.CurrentRow.Cells["colProductId"].Value;
                string name = dgvProducts.CurrentRow.Cells["colProductName"].Value.ToString();
                _presenter.DeleteProduct(id, name);
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

                txtId.Text = row.Cells["colProductId"].Value.ToString();
                txtName.Text = row.Cells["colProductName"].Value.ToString();
                numPrice.Value = (decimal)row.Cells["colProductPrice"].Value;
                numcolQuantity.Value = (int)row.Cells["colQuantity"].Value;
                numcolCostPrice.Value = (decimal)row.Cells["colCostPrice"].Value;
                cbCategory.SelectedValue = Convert.ToInt32(row.Cells["colCategoryId"].Value);
                cbSupplier.SelectedValue = Convert.ToInt32(row.Cells["colSupplierId"].Value);
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
            numcolCostPrice.Value = 0;
            numcolQuantity.Value = 0;
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
                ShowMessage("Please select an action from the dropdown first.");
                return;
            }

            if (action == "New Product" || action == "Update Details")
            {
                if (action == "New Product") txtId.Text = "0";

                _presenter.SaveProduct();
                clearInputFields();
            }
            else if (action == "IN" || action == "OUT" || action == "ADJ")
            {
                _presenter.MakeTransaction();
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

        public void LoadCategories(IEnumerable<Category> categories)
        {
            cbCategory.DataSource = null;

            cbCategory.DisplayMember = "Category";
            cbCategory.ValueMember = "CategoryId";

            if (categories != null && categories.Any())
            {
                cbCategory.DataSource = categories.ToList();
            }
        }

        public void LoadSuppliers(IEnumerable<Supplier> suppliers)
        {
            cbSupplier.DataSource = null;
            cbSupplier.DisplayMember = "CompanyName";
            cbSupplier.ValueMember = "SupplierId";

            if (suppliers != null && suppliers.Any())
            {
                cbSupplier.DataSource = suppliers.ToList();
            }
        }

    }
}
