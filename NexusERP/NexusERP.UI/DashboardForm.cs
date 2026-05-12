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

namespace Product_Inventory_Manager.Product_Inventory_Manager.Views
{
    public partial class DashboardForm : Form, IDashboardView
    {
        private DashboardPresenter _presenter;
        public DashboardForm(DashboardPresenter presenter)
        {
            InitializeComponent();

            _presenter = presenter;
            _presenter.SetView(this);
        }


        public string TotalValue { set => TotalValueNumbox.Text = value; }
        public string TotalProfitValue { set => GrossPotentialProfitnum.Text = value; }
        public string MarginValue { set => OperatingMarginnum.Text = value; }
        public string LowStrockCount { set => LowStockCountnum.Text = value; }
        public string InventoryHealth { set => lblInventoryHealthStatus.Text = value; }
        public Color HealthColor { set => HealthStatuspanel.BackColor = value; }


        private void DashboardForm_Load(object sender, EventArgs args)
        {
            _presenter.LoadStatistics();
        }

    }
}
