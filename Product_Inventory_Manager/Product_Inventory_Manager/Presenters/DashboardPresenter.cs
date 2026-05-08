using Product_Inventory_Manager.Product_Inventory_Manager.Views.Interfaces;
using Product_Inventory_Manager.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product_Inventory_Manager.Product_Inventory_Manager.Presenters
{
    internal class DashboardPresenter
    {
        private readonly IProductRepository _repository;
        private readonly IDashboardView _view;

        public DashboardPresenter(IDashboardView view, IProductRepository repository)
        {
            _view = view;
            _repository = repository;
        }
        public void LoadStatistics()
        {
            DataTable dt = _repository.getAll();

            decimal totalValue = 0;
            decimal totalProfit = 0;
            decimal totalCost = 0;
            int lowStrockCount = 0;

            foreach (DataRow row in dt.Rows)
            {
                decimal price = Convert.ToDecimal(row["ProductPrice"]);
                decimal cost = Convert.ToDecimal(row["CostPrice"]);
                int qty = Convert.ToInt32(row["Quantity"]);

                totalValue += (price * qty);
                totalCost += (cost * qty);
                totalProfit += (price - cost) * qty;
                if (qty < 5) lowStrockCount++;
            }

            decimal margin = totalValue > 0 ? (totalProfit / totalValue) * 100 : 0;

            _view.totalValue = totalValue.ToString("C");
            _view.totalProfitValue = totalProfit.ToString("C");
            _view.lowStrockCount = lowStrockCount.ToString();
            _view.marginValue = margin.ToString("F3") + "%";

            if (margin > 30 && lowStrockCount == 0)
            {
                _view.inventoryHealth = "EXELLENT";
                _view.healthColor = Color.Green;
            }
            else if (lowStrockCount > 0)
            {
                _view.inventoryHealth = "ACTION REQUIRED";
                _view.healthColor = Color.Orange;
            }
            else
            {
                _view.inventoryHealth = "STABLE";
                _view.healthColor = Color.DodgerBlue;
            }
        }
    }
}
