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
using NexusERP.Domain.Enums;
using NexusERP.Domain.State;

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

            this.dgvProducts.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvProducts_CellFormatting);
            this.dgvProducts.MouseDown += new MouseEventHandler(this.dgvProducts_MouseDown);
            this.Click += new EventHandler(this.Form1_Click);
            txtId.Enabled = false;

            ApplyInventoryFormRestrictions();

            _presenter.RefreshData();
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
        public int ProductQuantity { get => (int)numQuantity.Value; set => numQuantity.Value = value; }

        public int SoldQty { get => (int)numSell.Value; set => numSell.Value = value; }
        public string TransactionType
        {
            get => cbTransaction.SelectedItem?.ToString();
            set
            {
                if (Enum.TryParse(typeof(TransactionAction), value, out var action))
                    cbTransaction.SelectedItem = action;
            }
        }


        private void ApplyInventoryFormRestrictions()
        {
            var currentRole = NexusERP.Domain.State.UserSession.Role;

            if (currentRole == UserRole.Admin)
            {
                cbTransaction.DataSource = Enum.GetValues(typeof(TransactionAction));
                btnDelete.Enabled = true;
            }
            else if (currentRole == UserRole.Manager)
            {
                cbTransaction.DataSource = Enum.GetValues(typeof(TransactionAction));
                btnDelete.Enabled = false;
            }
            else
            {
                cbTransaction.DataSource = new[] { TransactionAction.Sale };
                cbTransaction.SelectedItem = TransactionAction.Sale;
                btnDelete.Enabled = false;
            }
        }

        private void cbTransaction_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbTransaction.SelectedItem is TransactionAction action)
            {
                bool isModifyingProduct = (action == TransactionAction.Create || action == TransactionAction.Edit);
                bool isMovingStock = (action == TransactionAction.Sale || action == TransactionAction.Restock || action == TransactionAction.Adjustment);

                txtId.Enabled = isModifyingProduct;
                txtName.Enabled = isModifyingProduct;
                numPrice.Enabled = isModifyingProduct;
                numcolCostPrice.Enabled = isModifyingProduct;
                numQuantity.Enabled = isModifyingProduct;
                cbCategory.Enabled = isModifyingProduct;
                cbSupplier.Enabled = isModifyingProduct;


                numSell.Enabled = isMovingStock;

                if (action == TransactionAction.Create)
                {
                    ClearInputFields();
                }
            }
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
                numQuantity.Value = (int)row.Cells["colQuantity"].Value;
                numcolCostPrice.Value = (decimal)row.Cells["colCostPrice"].Value;
                cbCategory.SelectedValue = Convert.ToInt32(row.Cells["colCategoryId"].Value);
                cbSupplier.SelectedValue = Convert.ToInt32(row.Cells["colSupplierId"].Value);
            }
            else
            {
                ClearInputFields();
            }
        }

        public void ClearInputFields()
        {
            dgvProducts.CurrentCell = null;
            txtId.Text = "";
            txtName.Text = "";
            numPrice.Value = 0;
            numcolCostPrice.Value = 0;
            numQuantity.Value = 0;
            numSell.Value = 0;
            cbCategory.SelectedIndex = -1;
            cbSupplier.SelectedIndex = -1;
        }

        private void dgvProducts_MouseDown(object sender, MouseEventArgs e)
        {
            DataGridView.HitTestInfo hit = dgvProducts.HitTest(e.X, e.Y);

            if (hit.Type == DataGridViewHitTestType.None)
            {
                dgvProducts.ClearSelection();
                ClearInputFields();
            }
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            dgvProducts.ClearSelection();
            ClearInputFields();
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cbTransaction.SelectedItem == null)
            {
                ShowMessage("Please select an action from the dropdown first.");
                return;
            }

            var action = (TransactionAction)cbTransaction.SelectedItem;
            var currentRole = NexusERP.Domain.State.UserSession.Role;

            if (currentRole == UserRole.Cashier && action != TransactionAction.Sale)
            {
                ShowMessage("Security Exception: Cashiers are restricted to outbound sales transactions only.");
                return;
            }

            if (action == TransactionAction.Create || action == TransactionAction.Edit)
            {
                if (action == TransactionAction.Create)
                    txtId.Text = "0";

                _presenter.SaveProduct();
                ClearInputFields();
            }
            else if (action == TransactionAction.Sale || action == TransactionAction.Restock || action == TransactionAction.Adjustment)
            {
                _presenter.MakeTransaction();
                ClearInputFields();
            }
        }


        public void LoadCategories(IEnumerable<Category> categories)
        {
            cbCategory.DataSource = null;

            cbCategory.DisplayMember = "CategoryName";
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

        private void txtId_TextChanged(object sender, EventArgs e)
        {

        }

        private void dgvProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
