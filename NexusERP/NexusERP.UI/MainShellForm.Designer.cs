namespace NexusERP.UI
{
    partial class MainShellForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainShellForm));
            SideBarPanel = new FlowLayoutPanel();
            panel1 = new Panel();
            label1 = new Label();
            btnSideBar = new PictureBox();
            panel3 = new Panel();
            btnDashboard = new Button();
            button4 = new Button();
            button5 = new Button();
            panel4 = new Panel();
            btnInventory = new Button();
            button7 = new Button();
            button8 = new Button();
            panel5 = new Panel();
            btnSuppliers = new Button();
            button10 = new Button();
            button11 = new Button();
            panel6 = new Panel();
            btnReports = new Button();
            button13 = new Button();
            button14 = new Button();
            btnRegister = new Button();
            SideBarTimer = new System.Windows.Forms.Timer(components);
            TopPanel = new Panel();
            btnLogOut = new Button();
            lblUserInfo = new Label();
            lblModuleTitle = new Label();
            MainPanel = new Panel();
            SideBarPanel.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnSideBar).BeginInit();
            panel3.SuspendLayout();
            panel4.SuspendLayout();
            panel5.SuspendLayout();
            panel6.SuspendLayout();
            TopPanel.SuspendLayout();
            SuspendLayout();
            // 
            // SideBarPanel
            // 
            SideBarPanel.BackColor = Color.FromArgb(23, 32, 42);
            SideBarPanel.Controls.Add(panel1);
            SideBarPanel.Controls.Add(panel3);
            SideBarPanel.Controls.Add(panel4);
            SideBarPanel.Controls.Add(panel5);
            SideBarPanel.Controls.Add(panel6);
            SideBarPanel.Controls.Add(btnRegister);
            SideBarPanel.Dock = DockStyle.Left;
            SideBarPanel.Location = new Point(0, 81);
            SideBarPanel.Margin = new Padding(3, 4, 3, 4);
            SideBarPanel.MaximumSize = new Size(278, 786);
            SideBarPanel.MinimumSize = new Size(97, 786);
            SideBarPanel.Name = "SideBarPanel";
            SideBarPanel.Size = new Size(97, 786);
            SideBarPanel.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(label1);
            panel1.Controls.Add(btnSideBar);
            panel1.Location = new Point(3, 4);
            panel1.Margin = new Padding(3, 4, 3, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(302, 139);
            panel1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.White;
            label1.Location = new Point(98, 48);
            label1.Name = "label1";
            label1.Size = new Size(67, 25);
            label1.TabIndex = 1;
            label1.Text = "Menu";
            // 
            // btnSideBar
            // 
            btnSideBar.Cursor = Cursors.Hand;
            btnSideBar.Image = Properties.Resources.icons8_burger_bar_64;
            btnSideBar.Location = new Point(24, 30);
            btnSideBar.Margin = new Padding(3, 4, 3, 4);
            btnSideBar.Name = "btnSideBar";
            btnSideBar.Size = new Size(51, 64);
            btnSideBar.SizeMode = PictureBoxSizeMode.StretchImage;
            btnSideBar.TabIndex = 0;
            btnSideBar.TabStop = false;
            btnSideBar.Click += btnSideBar_Click;
            // 
            // panel3
            // 
            panel3.Controls.Add(btnDashboard);
            panel3.Controls.Add(button4);
            panel3.Controls.Add(button5);
            panel3.Location = new Point(3, 151);
            panel3.Margin = new Padding(3, 4, 3, 4);
            panel3.Name = "panel3";
            panel3.Size = new Size(272, 81);
            panel3.TabIndex = 2;
            // 
            // btnDashboard
            // 
            btnDashboard.FlatStyle = FlatStyle.Flat;
            btnDashboard.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnDashboard.ForeColor = Color.White;
            btnDashboard.Image = Properties.Resources.icons8_dashboard_24;
            btnDashboard.ImageAlign = ContentAlignment.MiddleLeft;
            btnDashboard.Location = new Point(-2, -4);
            btnDashboard.Margin = new Padding(3, 4, 3, 4);
            btnDashboard.Name = "btnDashboard";
            btnDashboard.Padding = new Padding(30, 0, 0, 0);
            btnDashboard.Size = new Size(292, 109);
            btnDashboard.TabIndex = 4;
            btnDashboard.Text = "        Dashboard";
            btnDashboard.TextAlign = ContentAlignment.MiddleLeft;
            btnDashboard.UseVisualStyleBackColor = true;
            btnDashboard.Click += btnDashboard_Click;
            // 
            // button4
            // 
            button4.FlatStyle = FlatStyle.Flat;
            button4.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button4.ForeColor = Color.White;
            button4.Image = Properties.Resources.icons8_home_32;
            button4.ImageAlign = ContentAlignment.MiddleLeft;
            button4.Location = new Point(-10, -14);
            button4.Margin = new Padding(3, 4, 3, 4);
            button4.Name = "button4";
            button4.Padding = new Padding(30, 0, 0, 0);
            button4.Size = new Size(331, 109);
            button4.TabIndex = 3;
            button4.Text = "        Home";
            button4.TextAlign = ContentAlignment.MiddleLeft;
            button4.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            button5.FlatStyle = FlatStyle.Flat;
            button5.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button5.ForeColor = Color.White;
            button5.Image = Properties.Resources.icons8_home_32;
            button5.ImageAlign = ContentAlignment.MiddleLeft;
            button5.Location = new Point(-10, -12);
            button5.Margin = new Padding(3, 4, 3, 4);
            button5.Name = "button5";
            button5.Padding = new Padding(30, 0, 0, 0);
            button5.Size = new Size(331, 109);
            button5.TabIndex = 2;
            button5.Text = "        Home";
            button5.TextAlign = ContentAlignment.MiddleLeft;
            button5.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            panel4.Controls.Add(btnInventory);
            panel4.Controls.Add(button7);
            panel4.Controls.Add(button8);
            panel4.Location = new Point(3, 240);
            panel4.Margin = new Padding(3, 4, 3, 4);
            panel4.Name = "panel4";
            panel4.Size = new Size(272, 81);
            panel4.TabIndex = 2;
            // 
            // btnInventory
            // 
            btnInventory.FlatStyle = FlatStyle.Flat;
            btnInventory.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnInventory.ForeColor = Color.White;
            btnInventory.Image = Properties.Resources.icons8_inventory_32;
            btnInventory.ImageAlign = ContentAlignment.MiddleLeft;
            btnInventory.Location = new Point(-2, -4);
            btnInventory.Margin = new Padding(3, 4, 3, 4);
            btnInventory.Name = "btnInventory";
            btnInventory.Padding = new Padding(30, 0, 0, 0);
            btnInventory.Size = new Size(292, 109);
            btnInventory.TabIndex = 4;
            btnInventory.Text = "        Inventory";
            btnInventory.TextAlign = ContentAlignment.MiddleLeft;
            btnInventory.UseVisualStyleBackColor = true;
            btnInventory.Click += btnInventory_Click;
            // 
            // button7
            // 
            button7.FlatStyle = FlatStyle.Flat;
            button7.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button7.ForeColor = Color.White;
            button7.Image = Properties.Resources.icons8_home_32;
            button7.ImageAlign = ContentAlignment.MiddleLeft;
            button7.Location = new Point(-10, -14);
            button7.Margin = new Padding(3, 4, 3, 4);
            button7.Name = "button7";
            button7.Padding = new Padding(30, 0, 0, 0);
            button7.Size = new Size(285, 109);
            button7.TabIndex = 3;
            button7.Text = "        Home";
            button7.TextAlign = ContentAlignment.MiddleLeft;
            button7.UseVisualStyleBackColor = true;
            // 
            // button8
            // 
            button8.FlatStyle = FlatStyle.Flat;
            button8.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button8.ForeColor = Color.White;
            button8.Image = Properties.Resources.icons8_home_32;
            button8.ImageAlign = ContentAlignment.MiddleLeft;
            button8.Location = new Point(-10, -12);
            button8.Margin = new Padding(3, 4, 3, 4);
            button8.Name = "button8";
            button8.Padding = new Padding(30, 0, 0, 0);
            button8.Size = new Size(331, 109);
            button8.TabIndex = 2;
            button8.Text = "        Home";
            button8.TextAlign = ContentAlignment.MiddleLeft;
            button8.UseVisualStyleBackColor = true;
            // 
            // panel5
            // 
            panel5.Controls.Add(btnSuppliers);
            panel5.Controls.Add(button10);
            panel5.Controls.Add(button11);
            panel5.Location = new Point(3, 329);
            panel5.Margin = new Padding(3, 4, 3, 4);
            panel5.Name = "panel5";
            panel5.Size = new Size(272, 81);
            panel5.TabIndex = 2;
            // 
            // btnSuppliers
            // 
            btnSuppliers.FlatStyle = FlatStyle.Flat;
            btnSuppliers.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnSuppliers.ForeColor = Color.White;
            btnSuppliers.Image = Properties.Resources.icons8_supplier_30;
            btnSuppliers.ImageAlign = ContentAlignment.MiddleLeft;
            btnSuppliers.Location = new Point(-2, -4);
            btnSuppliers.Margin = new Padding(3, 4, 3, 4);
            btnSuppliers.Name = "btnSuppliers";
            btnSuppliers.Padding = new Padding(30, 0, 0, 0);
            btnSuppliers.Size = new Size(304, 109);
            btnSuppliers.TabIndex = 4;
            btnSuppliers.Text = "        Suppliers";
            btnSuppliers.TextAlign = ContentAlignment.MiddleLeft;
            btnSuppliers.UseVisualStyleBackColor = true;
            btnSuppliers.Click += btnSuppliers_Click;
            // 
            // button10
            // 
            button10.FlatStyle = FlatStyle.Flat;
            button10.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button10.ForeColor = Color.White;
            button10.Image = Properties.Resources.icons8_home_32;
            button10.ImageAlign = ContentAlignment.MiddleLeft;
            button10.Location = new Point(-10, -14);
            button10.Margin = new Padding(3, 4, 3, 4);
            button10.Name = "button10";
            button10.Padding = new Padding(30, 0, 0, 0);
            button10.Size = new Size(331, 109);
            button10.TabIndex = 3;
            button10.Text = "        Home";
            button10.TextAlign = ContentAlignment.MiddleLeft;
            button10.UseVisualStyleBackColor = true;
            // 
            // button11
            // 
            button11.FlatStyle = FlatStyle.Flat;
            button11.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button11.ForeColor = Color.White;
            button11.Image = Properties.Resources.icons8_home_32;
            button11.ImageAlign = ContentAlignment.MiddleLeft;
            button11.Location = new Point(-10, -12);
            button11.Margin = new Padding(3, 4, 3, 4);
            button11.Name = "button11";
            button11.Padding = new Padding(30, 0, 0, 0);
            button11.Size = new Size(331, 109);
            button11.TabIndex = 2;
            button11.Text = "        Home";
            button11.TextAlign = ContentAlignment.MiddleLeft;
            button11.UseVisualStyleBackColor = true;
            // 
            // panel6
            // 
            panel6.Controls.Add(btnReports);
            panel6.Controls.Add(button13);
            panel6.Controls.Add(button14);
            panel6.Location = new Point(3, 418);
            panel6.Margin = new Padding(3, 4, 3, 4);
            panel6.Name = "panel6";
            panel6.Size = new Size(272, 81);
            panel6.TabIndex = 2;
            // 
            // btnReports
            // 
            btnReports.FlatStyle = FlatStyle.Flat;
            btnReports.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnReports.ForeColor = Color.White;
            btnReports.Image = Properties.Resources.icons8_reports_32;
            btnReports.ImageAlign = ContentAlignment.MiddleLeft;
            btnReports.Location = new Point(-2, -4);
            btnReports.Margin = new Padding(3, 4, 3, 4);
            btnReports.Name = "btnReports";
            btnReports.Padding = new Padding(30, 0, 0, 0);
            btnReports.Size = new Size(292, 109);
            btnReports.TabIndex = 4;
            btnReports.Text = "        Reports";
            btnReports.TextAlign = ContentAlignment.MiddleLeft;
            btnReports.UseVisualStyleBackColor = true;
            btnReports.Click += btnReports_Click;
            // 
            // button13
            // 
            button13.FlatStyle = FlatStyle.Flat;
            button13.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button13.ForeColor = Color.White;
            button13.Image = Properties.Resources.icons8_home_32;
            button13.ImageAlign = ContentAlignment.MiddleLeft;
            button13.Location = new Point(-10, -14);
            button13.Margin = new Padding(3, 4, 3, 4);
            button13.Name = "button13";
            button13.Padding = new Padding(30, 0, 0, 0);
            button13.Size = new Size(331, 109);
            button13.TabIndex = 3;
            button13.Text = "        Home";
            button13.TextAlign = ContentAlignment.MiddleLeft;
            button13.UseVisualStyleBackColor = true;
            // 
            // button14
            // 
            button14.FlatStyle = FlatStyle.Flat;
            button14.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button14.ForeColor = Color.White;
            button14.Image = Properties.Resources.icons8_home_32;
            button14.ImageAlign = ContentAlignment.MiddleLeft;
            button14.Location = new Point(-10, -12);
            button14.Margin = new Padding(3, 4, 3, 4);
            button14.Name = "button14";
            button14.Padding = new Padding(30, 0, 0, 0);
            button14.Size = new Size(331, 109);
            button14.TabIndex = 2;
            button14.Text = "        Home";
            button14.TextAlign = ContentAlignment.MiddleLeft;
            button14.UseVisualStyleBackColor = true;
            // 
            // btnRegister
            // 
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnRegister.ForeColor = Color.White;
            btnRegister.Image = (Image)resources.GetObject("btnRegister.Image");
            btnRegister.ImageAlign = ContentAlignment.MiddleLeft;
            btnRegister.Location = new Point(3, 507);
            btnRegister.Margin = new Padding(3, 4, 3, 4);
            btnRegister.Name = "btnRegister";
            btnRegister.Padding = new Padding(30, 0, 0, 0);
            btnRegister.Size = new Size(292, 109);
            btnRegister.TabIndex = 5;
            btnRegister.Text = "        Register";
            btnRegister.TextAlign = ContentAlignment.MiddleLeft;
            btnRegister.UseVisualStyleBackColor = true;
            btnRegister.Click += tbnRegister_Click;
            // 
            // SideBarTimer
            // 
            SideBarTimer.Interval = 10;
            SideBarTimer.Tick += SideBarTimer_Tick;
            // 
            // TopPanel
            // 
            TopPanel.BackColor = Color.FromArgb(23, 32, 42);
            TopPanel.Controls.Add(btnLogOut);
            TopPanel.Controls.Add(lblUserInfo);
            TopPanel.Controls.Add(lblModuleTitle);
            TopPanel.Dock = DockStyle.Top;
            TopPanel.Location = new Point(0, 0);
            TopPanel.Margin = new Padding(3, 4, 3, 4);
            TopPanel.Name = "TopPanel";
            TopPanel.Size = new Size(1499, 81);
            TopPanel.TabIndex = 1;
            // 
            // btnLogOut
            // 
            btnLogOut.Location = new Point(1350, 22);
            btnLogOut.Name = "btnLogOut";
            btnLogOut.Size = new Size(108, 36);
            btnLogOut.TabIndex = 7;
            btnLogOut.Text = "LogOut";
            btnLogOut.UseVisualStyleBackColor = true;
            btnLogOut.Click += btnLogOut_Click;
            // 
            // lblUserInfo
            // 
            lblUserInfo.AutoSize = true;
            lblUserInfo.Font = new Font("Cambria", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblUserInfo.ForeColor = Color.White;
            lblUserInfo.Location = new Point(775, 22);
            lblUserInfo.Name = "lblUserInfo";
            lblUserInfo.Size = new Size(133, 36);
            lblUserInfo.TabIndex = 6;
            lblUserInfo.Text = "User Info";
            // 
            // lblModuleTitle
            // 
            lblModuleTitle.AutoSize = true;
            lblModuleTitle.Font = new Font("Cambria", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblModuleTitle.ForeColor = Color.White;
            lblModuleTitle.Location = new Point(20, 11);
            lblModuleTitle.Name = "lblModuleTitle";
            lblModuleTitle.Size = new Size(171, 36);
            lblModuleTitle.TabIndex = 0;
            lblModuleTitle.Text = "PageHeader";
            // 
            // MainPanel
            // 
            MainPanel.Dock = DockStyle.Fill;
            MainPanel.Location = new Point(97, 81);
            MainPanel.Margin = new Padding(3, 4, 3, 4);
            MainPanel.Name = "MainPanel";
            MainPanel.Size = new Size(1402, 777);
            MainPanel.TabIndex = 2;
            // 
            // MainShellForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1499, 858);
            Controls.Add(MainPanel);
            Controls.Add(SideBarPanel);
            Controls.Add(TopPanel);
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainShellForm";
            Text = "MainShell";
            Load += MainShell_Load;
            SideBarPanel.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)btnSideBar).EndInit();
            panel3.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel6.ResumeLayout(false);
            TopPanel.ResumeLayout(false);
            TopPanel.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel SideBarPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnDashboard;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnInventory;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Button btnSuppliers;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Button btnReports;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.PictureBox btnSideBar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer SideBarTimer;
        private System.Windows.Forms.Panel TopPanel;
        private System.Windows.Forms.Label lblModuleTitle;
        private System.Windows.Forms.Panel MainPanel;
        private Button btnRegister;
        private Label lblUserInfo;
        private Button btnLogOut;
    }
}