using NexusERP.Application.Interfaces.Views;
using NexusERP.Application.Presenters;
using NexusERP.Domain.Enums;
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
    public partial class RegisterForm : Form, IRegisterView
    {
        private readonly RegisterPresenter _presenter;
        public RegisterForm(RegisterPresenter presenter)
        {
            InitializeComponent();
            _presenter = presenter;
            _presenter.SetView(this);

            cmbRole.DataSource = Enum.GetValues(typeof(UserRole));
        }

        public string Username { get => txtUsername.Text; }
        public string Password { get => txtPassword.Text; }
        public string FullName { get => txtFullName.Text; }

        public UserRole SelectRole => (UserRole)cmbRole.SelectedItem;

        public void ShowMessage(string message) => MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public void ShowError(string message) => MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private void btnRegister_Click(object sender, EventArgs e)
        {
            _presenter.Register();
        }
    }
}
