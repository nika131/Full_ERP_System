using NexusERP.Application.Interfaces.Views;
using NexusERP.Application.Presenters;
using NexusERP.Domain.Entities;
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
    public partial class EmployeesForm : Form, IEmployeeView
    {
        private readonly EmployeePresenter _presenter;
        public EmployeesForm(EmployeePresenter presenter)
        {
            InitializeComponent();

            _presenter = presenter;
            _presenter.SetView(this);

            SetupGrid();
            SetupDropdown();

            presenter.LoadAndFilter("", "All");
        }

        public IEnumerable<User> GridDataSource
        {
            set => dgvEmployees.DataSource = value.ToList();
        }

        public void ShowMessage(string message) => MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        public void ShowError(string message) => MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);


        private void SetupGrid()
        {
            dgvEmployees.AutoGenerateColumns = true;
            dgvEmployees.AllowUserToAddRows = false;
            dgvEmployees.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvEmployees.MultiSelect = false;
            dgvEmployees.ReadOnly = true;
        }

        private void SetupDropdown()
        {
            cbRoleFilter.Items.Add("All");
            foreach (var role in Enum.GetValues(typeof(UserRole)))
            {
                cbRoleFilter.Items.Add(role.ToString());
                cbEditRole.Items.Add(role.ToString());
            }
            cbRoleFilter.SelectedIndex = 0;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            _presenter.LoadAndFilter(txtSearch.Text, cbRoleFilter.SelectedItem?.ToString() ?? "All");
        }

        private void cbRoleFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            _presenter.LoadAndFilter(txtSearch.Text, cbRoleFilter.SelectedItem?.ToString() ?? "All");
        }

        private void dgvEmployees_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentCell != null)
            {
                var selectedUser = (User)dgvEmployees.CurrentRow.DataBoundItem;

                txtFullName.Text = selectedUser.FullName;
                txtUsername.Text = selectedUser.Username;
                cbEditRole.SelectedItem = selectedUser.Role.ToString();
            }
        }

        private void SaveChanges_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentRow == null) return;

            var selectedUser = (User)dgvEmployees.CurrentRow.DataBoundItem;

            selectedUser.FullName = txtFullName.Text;
            selectedUser.Username = txtUsername.Text;
            selectedUser.Role = Enum.Parse<UserRole>(cbEditRole.SelectedItem.ToString());

            _presenter.UpdateEmployee(selectedUser);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentRow == null)
            {
                ShowError("Please select an employee to revoke access.");
                return;
            }

            var selectedUser = (User)dgvEmployees.CurrentRow.DataBoundItem;

            var confirm = MessageBox.Show(
                $"Are you sure you want to revoke system access for {selectedUser.FullName}?",
                "Confirm Access Revocation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                _presenter.DeleteEmployee(selectedUser.UserId);

                _presenter.LoadAndFilter(txtSearch.Text, cbRoleFilter.SelectedItem?.ToString() ?? "All");
            }
        }
    }
}
