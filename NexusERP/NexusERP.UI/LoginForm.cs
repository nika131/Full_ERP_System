using Microsoft.Extensions.DependencyInjection;
using NexusERP.Application.Interfaces.Views;
using NexusERP.Application.Presenters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NexusERP.UI
{
    public partial class LoginForm : Form, ILoginView
    {
        private readonly LoginPresenter _presenter;

        public LoginForm(LoginPresenter presenter)
        {
            InitializeComponent();
            _presenter = presenter;
            _presenter.SetView(this);
        }

        public string Username { get => txtUsername.Text; }
        public string Password { get => txtPassword.Text; } 

        public void ShowMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void HideView()
        {
            this.Hide();
        }

        public void OpenMainDashboard()
        {
            var mainForm = Program.serviceProvider.GetRequiredService<MainShellForm>();

            mainForm.FormClosed += (s, args) => System.Windows.Forms.Application.Exit();

            mainForm.Show();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            _presenter.Login();
        }
    }
}
