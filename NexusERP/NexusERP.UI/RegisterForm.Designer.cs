namespace NexusERP.UI
{
    partial class RegisterForm
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
            label1 = new Label();
            label2 = new Label();
            txtFullName = new TextBox();
            txtUsername = new TextBox();
            label3 = new Label();
            txtPassword = new TextBox();
            label4 = new Label();
            label5 = new Label();
            cmbRole = new ComboBox();
            btnRegister = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Verdana", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.White;
            label1.Location = new Point(120, 50);
            label1.Name = "label1";
            label1.Size = new Size(244, 36);
            label1.TabIndex = 0;
            label1.Text = "Add new user";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Verdana", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.White;
            label2.Location = new Point(120, 115);
            label2.Name = "label2";
            label2.Size = new Size(79, 18);
            label2.TabIndex = 1;
            label2.Text = "Full name";
            // 
            // txtFullName
            // 
            txtFullName.Location = new Point(120, 138);
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(176, 27);
            txtFullName.TabIndex = 2;
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(120, 210);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(176, 27);
            txtUsername.TabIndex = 4;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Verdana", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.ForeColor = Color.White;
            label3.Location = new Point(120, 187);
            label3.Name = "label3";
            label3.Size = new Size(84, 18);
            label3.TabIndex = 3;
            label3.Text = "Username";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(120, 276);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(176, 27);
            txtPassword.TabIndex = 6;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Verdana", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.ForeColor = Color.White;
            label4.Location = new Point(120, 253);
            label4.Name = "label4";
            label4.Size = new Size(80, 18);
            label4.TabIndex = 5;
            label4.Text = "Password";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Verdana", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label5.ForeColor = Color.White;
            label5.Location = new Point(120, 317);
            label5.Name = "label5";
            label5.Size = new Size(40, 18);
            label5.TabIndex = 7;
            label5.Text = "Role";
            // 
            // cmbRole
            // 
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRole.FormattingEnabled = true;
            cmbRole.Location = new Point(120, 337);
            cmbRole.Name = "cmbRole";
            cmbRole.Size = new Size(176, 28);
            cmbRole.TabIndex = 8;
            // 
            // btnRegister
            // 
            btnRegister.Location = new Point(120, 394);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(131, 29);
            btnRegister.TabIndex = 9;
            btnRegister.Text = "Add";
            btnRegister.UseVisualStyleBackColor = true;
            btnRegister.Click += btnRegister_Click;
            // 
            // RegisterForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(23, 32, 42);
            ClientSize = new Size(800, 450);
            Controls.Add(btnRegister);
            Controls.Add(cmbRole);
            Controls.Add(label5);
            Controls.Add(txtPassword);
            Controls.Add(label4);
            Controls.Add(txtUsername);
            Controls.Add(label3);
            Controls.Add(txtFullName);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "RegisterForm";
            Text = "RegisterForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox txtFullName;
        private TextBox txtUsername;
        private Label label3;
        private TextBox txtPassword;
        private Label label4;
        private Label label5;
        private ComboBox cmbRole;
        private Button btnRegister;
    }
}