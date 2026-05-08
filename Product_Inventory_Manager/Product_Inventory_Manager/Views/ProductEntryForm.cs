using Product_Inventory_Manager.Data;
using Product_Inventory_Manager.Presenters;
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
    public partial class ProductEntryForm : Form, IProductEntryView
    {
        private ProductEntryPresenter _presenter;

        public ProductEntryForm()
        {
            InitializeComponent();
            _presenter = new ProductEntryPresenter(this, new ProductRepository());
        }


        public string productName { get => txtName.Text; set => txtName.Text = value; }
        public decimal productPrice { get => numPrice.Value; set => numPrice.Value = value; }
        public int productQuantity { get => (int)numQuantity.Value; set => numQuantity.Value = value; }
        private int _initialCatId;
        public int initialCategoryId { get => _initialCatId; set => _initialCatId = value; }
        public int categoryId
        {
            get => cbCategory.SelectedValue != null ? (int)cbCategory.SelectedValue : _initialCatId;
            set => cbCategory.SelectedValue = value;
        }
        public int productId { get; set; } = 0;
        public decimal costPrice { get => numCostPrice.Value; set => numCostPrice.Value = value; }
        private int _initialSupId;
        public int initialSupplierId { get => _initialSupId; set => _initialSupId = value; }
        public int supplierId {
            get => cbSupplier.SelectedValue != null ? (int)cbSupplier.SelectedValue : _initialSupId;
            set => cbSupplier.SelectedValue = value;    
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            _presenter.saveProduct();
        }

        public void showMessage(string message) => MessageBox.Show(message);
        public void closeView() => this.DialogResult = DialogResult.OK;

        public void loadCategories(DataTable categories)
        {
            cbCategory.DataSource = categories;
            cbCategory.DisplayMember = "Name";
            cbCategory.ValueMember = "CategoryId";
        }

        public void loadSuppliers(DataTable suppliers)
        {
            cbSupplier.DataSource = suppliers;
            cbSupplier.DisplayMember = "Name";
            cbSupplier.ValueMember = "SupplierId";
        }
        private void ProductEntryForm_Load(object sender, EventArgs e)
        {
            _presenter.showInitializationData(this.initialCategoryId, this.initialSupplierId);
        }

        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
