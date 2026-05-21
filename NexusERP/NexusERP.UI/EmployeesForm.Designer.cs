namespace NexusERP.UI
{
    partial class EmployeesForm
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
            txtSearch = new TextBox();
            cbRoleFilter = new ComboBox();
            dgvEmployees = new DataGridView();
            txtFullName = new TextBox();
            txtUsername = new TextBox();
            cbEditRole = new ComboBox();
            btnDelete = new Button();
            Search = new Label();
            label2 = new Label();
            label3 = new Label();
            label5 = new Label();
            label6 = new Label();
            SaveChanges = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvEmployees).BeginInit();
            SuspendLayout();
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(585, 35);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(125, 27);
            txtSearch.TabIndex = 0;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // cbRoleFilter
            // 
            cbRoleFilter.FormattingEnabled = true;
            cbRoleFilter.Location = new Point(742, 34);
            cbRoleFilter.Name = "cbRoleFilter";
            cbRoleFilter.Size = new Size(151, 28);
            cbRoleFilter.TabIndex = 1;
            cbRoleFilter.SelectedIndexChanged += cbRoleFilter_SelectedIndexChanged;
            // 
            // dgvEmployees
            // 
            dgvEmployees.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvEmployees.Location = new Point(12, 12);
            dgvEmployees.Name = "dgvEmployees";
            dgvEmployees.RowHeadersWidth = 51;
            dgvEmployees.Size = new Size(480, 323);
            dgvEmployees.TabIndex = 2;
            dgvEmployees.SelectionChanged += dgvEmployees_SelectionChanged;
            // 
            // txtFullName
            // 
            txtFullName.Location = new Point(585, 136);
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(125, 27);
            txtFullName.TabIndex = 3;
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(742, 136);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(125, 27);
            txtUsername.TabIndex = 4;
            // 
            // cbEditRole
            // 
            cbEditRole.FormattingEnabled = true;
            cbEditRole.Location = new Point(585, 193);
            cbEditRole.Name = "cbEditRole";
            cbEditRole.Size = new Size(151, 28);
            cbEditRole.TabIndex = 6;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(764, 269);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(94, 29);
            btnDelete.TabIndex = 8;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // Search
            // 
            Search.AutoSize = true;
            Search.Location = new Point(585, 9);
            Search.Name = "Search";
            Search.Size = new Size(50, 20);
            Search.TabIndex = 9;
            Search.Text = "label1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(742, 9);
            label2.Name = "label2";
            label2.Size = new Size(42, 20);
            label2.TabIndex = 10;
            label2.Text = "Filter";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(585, 113);
            label3.Name = "label3";
            label3.Size = new Size(76, 20);
            label3.TabIndex = 11;
            label3.Text = "Full Name";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(742, 113);
            label5.Name = "label5";
            label5.Size = new Size(75, 20);
            label5.TabIndex = 13;
            label5.Text = "Username";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(585, 171);
            label6.Name = "label6";
            label6.Size = new Size(39, 20);
            label6.TabIndex = 14;
            label6.Text = "Role";
            // 
            // SaveChanges
            // 
            SaveChanges.Location = new Point(616, 269);
            SaveChanges.Name = "SaveChanges";
            SaveChanges.Size = new Size(94, 29);
            SaveChanges.TabIndex = 15;
            SaveChanges.Text = "Save";
            SaveChanges.UseVisualStyleBackColor = true;
            SaveChanges.Click += SaveChanges_Click;
            // 
            // EmployeesForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(961, 450);
            Controls.Add(SaveChanges);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(Search);
            Controls.Add(btnDelete);
            Controls.Add(cbEditRole);
            Controls.Add(txtUsername);
            Controls.Add(txtFullName);
            Controls.Add(dgvEmployees);
            Controls.Add(cbRoleFilter);
            Controls.Add(txtSearch);
            Name = "EmployeesForm";
            Text = "EmployeesForm";
            ((System.ComponentModel.ISupportInitialize)dgvEmployees).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtSearch;
        private ComboBox cbRoleFilter;
        private DataGridView dgvEmployees;
        private TextBox txtFullName;
        private TextBox txtUsername;
        private TextBox textBox4;
        private ComboBox cbEditRole;
        private Button button1;
        private Button btnDelete;
        private Label Search;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Button SaveChanges;
    }
}