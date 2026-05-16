using System;
using System.Collections.Generic;

namespace NexusERP.UI
{
    partial class ProductForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }



        private void InitializeComponent()
        {
            dgvProducts = new DataGridView();
            colProductId = new DataGridViewTextBoxColumn();
            colProductName = new DataGridViewTextBoxColumn();
            Category = new DataGridViewTextBoxColumn();
            colCategoryId = new DataGridViewTextBoxColumn();
            colProductPrice = new DataGridViewTextBoxColumn();
            colQuantity = new DataGridViewTextBoxColumn();
            colCostPrice = new DataGridViewTextBoxColumn();
            colSupplierId = new DataGridViewTextBoxColumn();
            txtSearch = new TextBox();
            label1 = new Label();
            lblTotalItems = new Label();
            lblTotalValue = new Label();
            lblLowStock = new Label();
            lblTotalProfit = new Label();
            label6 = new Label();
            label5 = new Label();
            cbSupplier = new ComboBox();
            numcolCostPrice = new NumericUpDown();
            label4 = new Label();
            cbTransaction = new ComboBox();
            label3 = new Label();
            label2 = new Label();
            label7 = new Label();
            numPrice = new NumericUpDown();
            numQuantity = new NumericUpDown();
            txtName = new TextBox();
            lblSell = new Label();
            numSell = new NumericUpDown();
            lblCategory = new Label();
            cbCategory = new ComboBox();
            lblId = new Label();
            txtId = new TextBox();
            btnSave = new Button();
            btnDelete = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvProducts).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numcolCostPrice).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPrice).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numQuantity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSell).BeginInit();
            SuspendLayout();
            // 
            // dgvProducts
            // 
            dgvProducts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProducts.Columns.AddRange(new DataGridViewColumn[] { colProductId, colProductName, Category, colCategoryId, colProductPrice, colQuantity, colCostPrice, colSupplierId });
            dgvProducts.Location = new Point(12, 19);
            dgvProducts.Name = "dgvProducts";
            dgvProducts.RowHeadersWidth = 51;
            dgvProducts.RowTemplate.Height = 24;
            dgvProducts.Size = new Size(802, 408);
            dgvProducts.TabIndex = 0;
            dgvProducts.CellClick += dgvProducts_CellClick;
            dgvProducts.MouseDown += dgvProducts_MouseDown;
            // 
            // colProductId
            // 
            colProductId.DataPropertyName = "ProductId";
            colProductId.HeaderText = "ProductId";
            colProductId.MinimumWidth = 6;
            colProductId.Name = "colProductId";
            colProductId.Width = 125;
            // 
            // colProductName
            // 
            colProductName.DataPropertyName = "ProductName";
            colProductName.HeaderText = "ProductName";
            colProductName.MinimumWidth = 6;
            colProductName.Name = "colProductName";
            colProductName.Width = 125;
            // 
            // Category
            // 
            Category.DataPropertyName = "Category";
            Category.HeaderText = "Category";
            Category.MinimumWidth = 6;
            Category.Name = "Category";
            Category.Width = 125;
            // 
            // colCategoryId
            // 
            colCategoryId.DataPropertyName = "ProductCategoryId";
            colCategoryId.HeaderText = "CategoryId";
            colCategoryId.MinimumWidth = 6;
            colCategoryId.Name = "colCategoryId";
            colCategoryId.Width = 125;
            // 
            // colProductPrice
            // 
            colProductPrice.DataPropertyName = "ProductPrice";
            colProductPrice.HeaderText = "ProductPrice";
            colProductPrice.MinimumWidth = 6;
            colProductPrice.Name = "colProductPrice";
            colProductPrice.Width = 125;
            // 
            // colQuantity
            // 
            colQuantity.DataPropertyName = "Quantity";
            colQuantity.HeaderText = "Quantity";
            colQuantity.MinimumWidth = 6;
            colQuantity.Name = "colQuantity";
            colQuantity.Width = 125;
            // 
            // colCostPrice
            // 
            colCostPrice.DataPropertyName = "ProductCostPrice";
            colCostPrice.HeaderText = "CostPrice";
            colCostPrice.MinimumWidth = 6;
            colCostPrice.Name = "colCostPrice";
            colCostPrice.Width = 125;
            // 
            // colSupplierId
            // 
            colSupplierId.DataPropertyName = "SupplierId";
            colSupplierId.HeaderText = "SupplierId";
            colSupplierId.MinimumWidth = 6;
            colSupplierId.Name = "colSupplierId";
            colSupplierId.Width = 125;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(838, 41);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(208, 27);
            txtSearch.TabIndex = 2;
            txtSearch.TextChanged += txtSearch_TextChanged_1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(838, 19);
            label1.Name = "label1";
            label1.Size = new Size(107, 20);
            label1.TabIndex = 6;
            label1.Text = "search product";
            // 
            // lblTotalItems
            // 
            lblTotalItems.AutoSize = true;
            lblTotalItems.Location = new Point(26, 520);
            lblTotalItems.Name = "lblTotalItems";
            lblTotalItems.Size = new Size(50, 20);
            lblTotalItems.TabIndex = 7;
            lblTotalItems.Text = "label2";
            // 
            // lblTotalValue
            // 
            lblTotalValue.AutoSize = true;
            lblTotalValue.Location = new Point(183, 520);
            lblTotalValue.Name = "lblTotalValue";
            lblTotalValue.Size = new Size(50, 20);
            lblTotalValue.TabIndex = 8;
            lblTotalValue.Text = "label3";
            // 
            // lblLowStock
            // 
            lblLowStock.AutoSize = true;
            lblLowStock.Location = new Point(609, 520);
            lblLowStock.Name = "lblLowStock";
            lblLowStock.Size = new Size(50, 20);
            lblLowStock.TabIndex = 9;
            lblLowStock.Text = "label4";
            // 
            // lblTotalProfit
            // 
            lblTotalProfit.AutoSize = true;
            lblTotalProfit.Location = new Point(333, 520);
            lblTotalProfit.Name = "lblTotalProfit";
            lblTotalProfit.Size = new Size(50, 20);
            lblTotalProfit.TabIndex = 10;
            lblTotalProfit.Text = "label4";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(840, 304);
            label6.Name = "label6";
            label6.Size = new Size(74, 20);
            label6.TabIndex = 27;
            label6.Text = "Cost Price";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(975, 175);
            label5.Name = "label5";
            label5.Size = new Size(64, 20);
            label5.TabIndex = 26;
            label5.Text = "Supplier";
            // 
            // cbSupplier
            // 
            cbSupplier.FormattingEnabled = true;
            cbSupplier.Location = new Point(978, 198);
            cbSupplier.Name = "cbSupplier";
            cbSupplier.Size = new Size(121, 28);
            cbSupplier.TabIndex = 25;
            // 
            // numcolCostPrice
            // 
            numcolCostPrice.DecimalPlaces = 2;
            numcolCostPrice.Location = new Point(843, 331);
            numcolCostPrice.Name = "numcolCostPrice";
            numcolCostPrice.Size = new Size(120, 27);
            numcolCostPrice.TabIndex = 24;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(975, 394);
            label4.Name = "label4";
            label4.Size = new Size(117, 20);
            label4.TabIndex = 23;
            label4.Text = "Transaction type";
            // 
            // cbTransaction
            // 
            cbTransaction.FormattingEnabled = true;
            cbTransaction.Items.AddRange(new object[] { "IN", "OUT", "ADJ", "New Product", "Update Details" });
            cbTransaction.Location = new Point(978, 417);
            cbTransaction.Name = "cbTransaction";
            cbTransaction.Size = new Size(121, 28);
            cbTransaction.TabIndex = 22;
            cbTransaction.SelectedIndexChanged += cbTransaction_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(840, 242);
            label3.Name = "label3";
            label3.Size = new Size(42, 20);
            label3.TabIndex = 21;
            label3.Text = "price";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(973, 240);
            label2.Name = "label2";
            label2.Size = new Size(65, 20);
            label2.TabIndex = 20;
            label2.Text = "Quantity";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(975, 115);
            label7.Name = "label7";
            label7.Size = new Size(46, 20);
            label7.TabIndex = 19;
            label7.Text = "name";
            // 
            // numPrice
            // 
            numPrice.DecimalPlaces = 2;
            numPrice.Location = new Point(843, 267);
            numPrice.Name = "numPrice";
            numPrice.Size = new Size(120, 27);
            numPrice.TabIndex = 18;
            // 
            // numQuantity
            // 
            numQuantity.Location = new Point(976, 267);
            numQuantity.Name = "numQuantity";
            numQuantity.Size = new Size(120, 27);
            numQuantity.TabIndex = 17;
            // 
            // txtName
            // 
            txtName.Location = new Point(976, 140);
            txtName.Name = "txtName";
            txtName.Size = new Size(100, 27);
            txtName.TabIndex = 16;
            // 
            // lblSell
            // 
            lblSell.AutoSize = true;
            lblSell.Location = new Point(839, 392);
            lblSell.Name = "lblSell";
            lblSell.Size = new Size(94, 20);
            lblSell.TabIndex = 29;
            lblSell.Text = "Sold amount";
            // 
            // numSell
            // 
            numSell.Location = new Point(842, 419);
            numSell.Name = "numSell";
            numSell.Size = new Size(120, 27);
            numSell.TabIndex = 28;
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.Location = new Point(838, 175);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(69, 20);
            lblCategory.TabIndex = 31;
            lblCategory.Text = "Category";
            // 
            // cbCategory
            // 
            cbCategory.FormattingEnabled = true;
            cbCategory.Location = new Point(841, 198);
            cbCategory.Name = "cbCategory";
            cbCategory.Size = new Size(121, 28);
            cbCategory.TabIndex = 30;
            // 
            // lblId
            // 
            lblId.AutoSize = true;
            lblId.Location = new Point(840, 115);
            lblId.Name = "lblId";
            lblId.Size = new Size(24, 20);
            lblId.TabIndex = 33;
            lblId.Text = "ID";
            // 
            // txtId
            // 
            txtId.Location = new Point(841, 140);
            txtId.Name = "txtId";
            txtId.Size = new Size(100, 27);
            txtId.TabIndex = 32;
            txtId.TextChanged += txtId_TextChanged;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(838, 475);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(124, 37);
            btnSave.TabIndex = 34;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(978, 475);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(124, 37);
            btnDelete.TabIndex = 35;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click_1;
            // 
            // ProductForm
            // 
            ClientSize = new Size(1603, 671);
            Controls.Add(btnDelete);
            Controls.Add(btnSave);
            Controls.Add(lblId);
            Controls.Add(txtId);
            Controls.Add(lblCategory);
            Controls.Add(cbCategory);
            Controls.Add(lblSell);
            Controls.Add(numSell);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(cbSupplier);
            Controls.Add(numcolCostPrice);
            Controls.Add(label4);
            Controls.Add(cbTransaction);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label7);
            Controls.Add(numPrice);
            Controls.Add(numQuantity);
            Controls.Add(txtName);
            Controls.Add(lblTotalProfit);
            Controls.Add(lblLowStock);
            Controls.Add(lblTotalValue);
            Controls.Add(lblTotalItems);
            Controls.Add(label1);
            Controls.Add(txtSearch);
            Controls.Add(dgvProducts);
            Name = "ProductForm";
            Click += Form1_Click;
            ((System.ComponentModel.ISupportInitialize)dgvProducts).EndInit();
            ((System.ComponentModel.ISupportInitialize)numcolCostPrice).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPrice).EndInit();
            ((System.ComponentModel.ISupportInitialize)numQuantity).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSell).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }



        private System.Windows.Forms.DataGridView dgvProducts;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblTotalItems;
        private System.Windows.Forms.Label lblTotalValue;
        private System.Windows.Forms.Label lblLowStock;
        private System.Windows.Forms.Label lblTotalProfit;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbSupplier;
        private System.Windows.Forms.NumericUpDown numcolCostPrice;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbTransaction;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numPrice;
        private System.Windows.Forms.NumericUpDown numQuantity;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblSell;
        private System.Windows.Forms.NumericUpDown numSell;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cbCategory;
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.TextBox txtId;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Category;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCategoryId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQuantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCostPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSupplierId;
    }
}

