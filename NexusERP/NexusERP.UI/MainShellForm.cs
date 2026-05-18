using Microsoft.Extensions.DependencyInjection;
using Product_Inventory_Manager.Product_Inventory_Manager.Views;
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
    public partial class MainShellForm : Form
    {
        bool sidebarExpanded;
        public MainShellForm()
        {
            InitializeComponent();

            lblUserInfo.Text = $"{NexusERP.Application.State.UserSession.Role} : {NexusERP.Application.State.UserSession.FullName}";
            
            ApplyPageRestrictions();
        }

        private void ApplyPageRestrictions()
        {
            var currentRole = NexusERP.Application.State.UserSession.Role;

            if (currentRole != NexusERP.Domain.Enums.UserRole.Admin && currentRole != NexusERP.Domain.Enums.UserRole.Manager)
            {
                btnSuppliers.Enabled = false;
                btnDashboard.Enabled = false;
                btnRegister.Enabled = false;
            }
            else if (currentRole != NexusERP.Domain.Enums.UserRole.Admin)
            {
                btnRegister.Enabled = false;
            }
        }

        private void OpenModule(Form moduleForm, string title)
        {
            lblModuleTitle.Text = title;

            if (MainPanel.Controls.Count > 0)
            {
                Control oldControl = MainPanel.Controls[0];

                MainPanel.Controls.Remove(oldControl);
                oldControl.Dispose();
            }


            moduleForm.TopLevel = false;
            moduleForm.FormBorderStyle = FormBorderStyle.None;
            moduleForm.Dock = DockStyle.Fill;

            MainPanel.Controls.Add(moduleForm);
            moduleForm.Show();
        }

        private void SideBarTimer_Tick(object sender, EventArgs e)
        {
            if (sidebarExpanded)
            {
                SideBarPanel.Width -= 10;
                if (SideBarPanel.Width == SideBarPanel.MinimumSize.Width)
                {
                    sidebarExpanded = false;
                    SideBarTimer.Stop();
                }
            }
            else
            {
                SideBarPanel.Width += 10;
                if (SideBarPanel.Width >= SideBarPanel.MaximumSize.Width)
                {
                    SideBarPanel.Width = SideBarPanel.MaximumSize.Width;
                    sidebarExpanded = true;
                    SideBarTimer.Stop();
                }
            }
        }


        private void MainShell_Load(object sender, EventArgs e)
        {
            btnInventory_Click(this, EventArgs.Empty);
        }
        private void btnSideBar_Click(object sender, EventArgs e)
        {
            SideBarTimer.Start();
        }

        private void btnInventory_Click(object sender, EventArgs e)
        {
            var ProductForm = Program.serviceProvider.GetRequiredService<ProductForm>();
            OpenModule(ProductForm, "Inventoy Manager");
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            var SupplierForm = Program.serviceProvider.GetRequiredService<SupplierForm>();
            OpenModule(SupplierForm, "Supplier Directory");
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            var DashboardForm = Program.serviceProvider.GetRequiredService<DashboardForm>();
            OpenModule(DashboardForm, "Bussines Overview");
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            var ReportsForm = Program.serviceProvider.GetRequiredService<ReportsForm>();
            OpenModule(ReportsForm, "Reports");
        }

        private void tbnRegister_Click(object sender, EventArgs e)
        {
            var RegisterForm = Program.serviceProvider.GetRequiredService<RegisterForm>();
            OpenModule(RegisterForm, "Register");
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "LogOut", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                NexusERP.Application.State.UserSession.Logout();

                System.Windows.Forms.Application.Restart();
            }
        }
    }
}
