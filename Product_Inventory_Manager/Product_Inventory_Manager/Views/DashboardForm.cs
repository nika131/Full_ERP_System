using Product_Inventory_Manager.Product_Inventory_Manager.Presenters;
using Product_Inventory_Manager.Product_Inventory_Manager.Views.Interfaces;
using Product_Inventory_Manager.Repositories;
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
    public partial class DashboardForm : Form, IDashboardView
    {
        private DashboardPresenter _presenter;
        public DashboardForm()
        {
            InitializeComponent();

            _presenter = new DashboardPresenter(this, new ProductRepository());
        }


        public string totalValue { set => TotalValueNumbox.Text = value; }
        public string totalProfitValue { set => GrossPotentialProfitnum.Text = value; }
        public string marginValue { set => OperatingMarginnum.Text = value; }
        public string lowStrockCount { set => LowStockCountnum.Text = value; }
        public string inventoryHealth { set => lblInventoryHealthStatus.Text = value; }
        public Color healthColor { set => HealthStatuspanel.BackColor = value; }


        private void DashboardForm_Load(object sender, EventArgs args)
        {
            _presenter.LoadStatistics();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }
    }
}
