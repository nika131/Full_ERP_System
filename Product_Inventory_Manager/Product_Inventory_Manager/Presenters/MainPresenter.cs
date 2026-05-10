using Product_Inventory_Manager.Data;
using Product_Inventory_Manager.Repositories;
using Product_Inventory_Manager.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Product_Inventory_Manager.Presenters
{
    public class MainPresenter
    {
        private readonly IMainView _view;
        private readonly IProductRepository _repository;

        public MainPresenter(IMainView view, IProductRepository repository)
        {
            _view = view;
            _repository = repository;

            _view.loadCategories(_repository.getCategories());
            _view.loadSuppliers(_repository.getSuppliers());
        }

        public void refreshData()
        {
            try
            {
                DataTable dt = _repository.getAll();
                _view.gridDataSource = dt;
                updateCalculations(dt);
            }
            catch (Exception ex)
            {
                _view.showError(ex.Message);
            }
        }

        public void search(string keyword)
        {
            try
            {
                DataTable dt = _repository.search(keyword);
                _view.gridDataSource = dt;
                updateCalculations(dt);
            }
            catch (Exception ex) 
            {
                _view.showError(ex.Message);
            }
        }

        public void updateCalculations(DataTable dt)
        {
            if (dt == null) return;

            int totalItmes = dt.Rows.Count;
            decimal totalValue = 0;
            int lowStock = 0;
            decimal totalPotentialProfit = 0;

            foreach (DataRow row in dt.Rows)
            {
                decimal price = Convert.ToDecimal(row["ProductPrice"]);
                decimal costPrice = Convert.ToDecimal(row["costPrice"]);
                int qty = Convert.ToInt32(row["Quantity"]);

                totalValue += (price * qty);
                totalPotentialProfit += ((price - costPrice) * qty);
                if (qty < 5) lowStock++;
            }

            _view.totalItemsText = $"Total Products: {totalItmes}";
            _view.totalValueText = $"Total Value: {totalValue:C}";
            _view.totalProfitText = $"Total Potential Profit: {totalPotentialProfit:C}";
            _view.lowStockText = $"Low Stock Alerts: {lowStock}";
            _view.lowStockColor = lowStock > 0 ? Color.Red : Color.Black;
        }

        public void deleteProduct (int id, string name)
        {
            if (_view.confirmDelete(name))
            {
                try
                {
                    _repository.delete(id);
                    refreshData();
                }
                catch (Exception ex)
                {
                    _view.showError(ex.Message);
                }
            }
        }

        public void makeTransaction()
        {
            if (string.IsNullOrWhiteSpace(_view.transactionType))
            {
                _view.showMessage("Transaction Type is Required");
                return;
            }

            if (_view.productId <= 0)
            {
                _view.showMessage("Please select a Product.");
                return;
            }

            if (_view.soldQty <= 0)
            {
                _view.showMessage("Sold quantity must be greater than zero.");
                return;
            }
            int finalqty = _view.transactionType == "OUT" ? (_view.soldQty * -1) : _view.soldQty;
            decimal unitPrice = _view.transactionType == "OUT" ? _view.productPrice : _view.costPrice;
            decimal Amount = unitPrice * _view.soldQty;
            try
            {
                _repository.makeTransaction(
                    _view.productId,
                    _view.supplierId,
                    _view.transactionType,
                    finalqty,
                    Amount
                );
            }
            catch (Exception ex)
            {
                _view.showMessage(ex.Message);
            }
        }

        public void saveProduct()
        {
            if (string.IsNullOrWhiteSpace(_view.productName))
            {
                _view.showMessage("Product Name is Required");
                return;
            }

            if (_view.categoryId <= 0)
            {
                _view.showMessage("Please select a category.");
                return;
            }

            if (_view.productPrice <= 0)
            {
                _view.showMessage("Price must be greater than zero.");
                return;
            }

            try
            {
                _repository.upSert(
                    _view.productId,
                    _view.productName,
                    _view.categoryId,
                    _view.productQuantity,
                    _view.productPrice,
                    _view.costPrice,
                    _view.supplierId
                );

            }
            catch (Exception ex)
            {
                _view.showMessage(ex.Message);
            }
        }
    }
}
