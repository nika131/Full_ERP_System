namespace NexusERP.UI
{
    partial class ReportsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgvReports = new DataGridView();
            label1 = new Label();
            txtSearch = new TextBox();
            lblId = new Label();
            txtId = new TextBox();
            btnExportExcel = new Button();
            btnExportPdf = new Button();
            TransactionId = new DataGridViewTextBoxColumn();
            ProductId = new DataGridViewTextBoxColumn();
            ProductName = new DataGridViewTextBoxColumn();
            Quantity = new DataGridViewTextBoxColumn();
            Amount = new DataGridViewTextBoxColumn();
            SupplierName = new DataGridViewTextBoxColumn();
            TransactionType = new DataGridViewTextBoxColumn();
            TransactionDate = new DataGridViewTextBoxColumn();
            SupplierId = new DataGridViewTextBoxColumn();
            Profit = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dgvReports).BeginInit();
            SuspendLayout();
            // 
            // dgvReports
            // 
            dgvReports.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvReports.Columns.AddRange(new DataGridViewColumn[] { TransactionId, ProductId, ProductName, Quantity, Amount, SupplierName, TransactionType, TransactionDate, SupplierId, Profit });
            dgvReports.Location = new Point(1, 1);
            dgvReports.Margin = new Padding(3, 4, 3, 4);
            dgvReports.Name = "dgvReports";
            dgvReports.RowHeadersWidth = 51;
            dgvReports.RowTemplate.Height = 24;
            dgvReports.Size = new Size(1047, 561);
            dgvReports.TabIndex = 0;
            dgvReports.CellClick += dgvReports_CellClick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(1138, 15);
            label1.Name = "label1";
            label1.Size = new Size(107, 20);
            label1.TabIndex = 8;
            label1.Text = "search product";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(1138, 42);
            txtSearch.Margin = new Padding(3, 4, 3, 4);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(208, 27);
            txtSearch.TabIndex = 7;
            // 
            // lblId
            // 
            lblId.AutoSize = true;
            lblId.Location = new Point(1140, 98);
            lblId.Name = "lblId";
            lblId.Size = new Size(125, 20);
            lblId.TabIndex = 35;
            lblId.Text = "Current Report ID";
            // 
            // txtId
            // 
            txtId.Location = new Point(1141, 129);
            txtId.Margin = new Padding(3, 4, 3, 4);
            txtId.Name = "txtId";
            txtId.Size = new Size(100, 27);
            txtId.TabIndex = 34;
            // 
            // btnExportExcel
            // 
            btnExportExcel.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnExportExcel.Location = new Point(1141, 196);
            btnExportExcel.Margin = new Padding(3, 4, 3, 4);
            btnExportExcel.Name = "btnExportExcel";
            btnExportExcel.Size = new Size(147, 39);
            btnExportExcel.TabIndex = 36;
            btnExportExcel.Text = "Export to Excel";
            btnExportExcel.UseVisualStyleBackColor = true;
            btnExportExcel.Click += btnExportExcel_Click;
            // 
            // btnExportPdf
            // 
            btnExportPdf.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnExportPdf.Location = new Point(1143, 261);
            btnExportPdf.Margin = new Padding(3, 4, 3, 4);
            btnExportPdf.Name = "btnExportPdf";
            btnExportPdf.Size = new Size(147, 39);
            btnExportPdf.TabIndex = 37;
            btnExportPdf.Text = "Export to PDF";
            btnExportPdf.UseVisualStyleBackColor = true;
            btnExportPdf.Click += btnExportPdf_Click;
            // 
            // TransactionId
            // 
            TransactionId.DataPropertyName = "TransactionId";
            TransactionId.HeaderText = "Transaction ID";
            TransactionId.MinimumWidth = 6;
            TransactionId.Name = "TransactionId";
            TransactionId.Width = 125;
            // 
            // ProductId
            // 
            ProductId.DataPropertyName = "ProductId";
            ProductId.HeaderText = "Product ID";
            ProductId.MinimumWidth = 6;
            ProductId.Name = "ProductId";
            ProductId.Width = 125;
            // 
            // ProductName
            // 
            ProductName.DataPropertyName = "ProductName";
            ProductName.HeaderText = "ProductName";
            ProductName.MinimumWidth = 6;
            ProductName.Name = "ProductName";
            ProductName.Width = 125;
            // 
            // Quantity
            // 
            Quantity.DataPropertyName = "Quantity";
            Quantity.HeaderText = "Quantity";
            Quantity.MinimumWidth = 6;
            Quantity.Name = "Quantity";
            Quantity.Width = 125;
            // 
            // Amount
            // 
            Amount.DataPropertyName = "Amount";
            Amount.HeaderText = "Amount Paid";
            Amount.MinimumWidth = 6;
            Amount.Name = "Amount";
            Amount.Width = 125;
            // 
            // SupplierName
            // 
            SupplierName.DataPropertyName = "SupplierName";
            SupplierName.HeaderText = "Supplier Name";
            SupplierName.MinimumWidth = 6;
            SupplierName.Name = "SupplierName";
            SupplierName.Width = 125;
            // 
            // TransactionType
            // 
            TransactionType.DataPropertyName = "TransactionType";
            TransactionType.HeaderText = "Transaction Type";
            TransactionType.MinimumWidth = 6;
            TransactionType.Name = "TransactionType";
            TransactionType.Width = 125;
            // 
            // TransactionDate
            // 
            TransactionDate.DataPropertyName = "TransactionDate";
            TransactionDate.HeaderText = "Transaction Date";
            TransactionDate.MinimumWidth = 6;
            TransactionDate.Name = "TransactionDate";
            TransactionDate.Width = 125;
            // 
            // SupplierId
            // 
            SupplierId.DataPropertyName = "SupplierId";
            SupplierId.HeaderText = "Supplier Id";
            SupplierId.MinimumWidth = 6;
            SupplierId.Name = "SupplierId";
            SupplierId.Width = 125;
            // 
            // Profit
            // 
            Profit.DataPropertyName = "Profit";
            Profit.HeaderText = "Profit";
            Profit.MinimumWidth = 6;
            Profit.Name = "Profit";
            Profit.Width = 125;
            // 
            // ReportsForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1391, 562);
            Controls.Add(btnExportPdf);
            Controls.Add(btnExportExcel);
            Controls.Add(lblId);
            Controls.Add(txtId);
            Controls.Add(label1);
            Controls.Add(txtSearch);
            Controls.Add(dgvReports);
            Margin = new Padding(3, 4, 3, 4);
            Name = "ReportsForm";
            Text = "ReportsForm";
            ((System.ComponentModel.ISupportInitialize)dgvReports).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvReports;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.TextBox txtId;
        private System.Windows.Forms.Button btnExportExcel;
        private System.Windows.Forms.Button btnExportPdf;
        private DataGridViewTextBoxColumn TransactionId;
        private DataGridViewTextBoxColumn ProductId;
        private DataGridViewTextBoxColumn ProductName;
        private DataGridViewTextBoxColumn Quantity;
        private DataGridViewTextBoxColumn Amount;
        private DataGridViewTextBoxColumn SupplierName;
        private DataGridViewTextBoxColumn TransactionType;
        private DataGridViewTextBoxColumn TransactionDate;
        private DataGridViewTextBoxColumn SupplierId;
        private DataGridViewTextBoxColumn Profit;
    }
}