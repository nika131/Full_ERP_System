using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Views;
using NexusERP.Domain.Entities;

namespace NexusERP.Application.Presenters
{
    public class DashboardPresenter
    {

        private IDashboardView _view = null!;
        private readonly IProductRepository _repository;

        public DashboardPresenter(IProductRepository repository)
        {
            _repository = repository;
        }

        public void SetView(IDashboardView view)
        {
            _view = view;
        }
        public void LoadStatistics()
        {
            IEnumerable<Product> products = _repository.GetAll();

            decimal totalValue = 0;
            decimal totalProfit = 0;
            decimal totalCost = 0;
            int lowStrockCount = 0;

            foreach (Product product in products)
            {
                decimal price = product.ProductPrice;
                decimal cost = product.ProductCostPrice;
                int qty = product.Quantity;

                totalValue += (price * qty);
                totalCost += (cost * qty);
                totalProfit += (price - cost) * qty;
                if (qty < 5) lowStrockCount++;
            }

            decimal margin = totalValue > 0 ? (totalProfit / totalValue) * 100 : 0;

            _view.TotalValue = totalValue.ToString("C");
            _view.TotalProfitValue = totalProfit.ToString("C");
            _view.LowStrockCount = lowStrockCount.ToString();
            _view.MarginValue = margin.ToString("F3") + "%";

            if (margin > 30 && lowStrockCount == 0)
            {
                _view.InventoryHealth = "EXELLENT";
            }
            else if (lowStrockCount > 0)
            {
                _view.InventoryHealth = "ACTION REQUIRED";
            }
            else
            {
                _view.InventoryHealth = "STABLE";
            }
        }
    }
}
