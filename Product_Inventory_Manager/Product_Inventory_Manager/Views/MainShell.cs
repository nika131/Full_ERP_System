using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Product_Inventory_Manager.Product_Inventory_Manager.Views
{
    public partial class MainShell : Form
    {
        bool sidebarExpanded;
        public MainShell()
        {
            InitializeComponent();
        }

        private void openModule(Form moduleForm, string title)
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

        private void btnSideBar_Click(object sender, EventArgs e)
        {
            SideBarTimer.Start();
        }

        private void btnInventory_Click(object sender, EventArgs e)
        {
            openModule(new Form1(), "Inventoy Manager");
        }

        private void btnSuppliers_Click(object sender, EventArgs e)
        {
            openModule(new SupplierForm(), "Supplier Directory");
        }

        private void MainShell_Load(object sender, EventArgs e)
        {
            btnDashboard_Click(this, EventArgs.Empty);
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            openModule(new DashboardForm(), "Bussines Overview");
        }
    }
}
