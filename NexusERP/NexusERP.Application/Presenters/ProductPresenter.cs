using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Views;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.State;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace NexusERP.Application.Presenters
{
    public class ProductPresenter
    {
        private IProductView _view;
        private readonly IProductRepository _repository;

        public ProductPresenter(IProductRepository repository)
        {
            _repository = repository;
        }

        public void SetView(IProductView view)
        {
            _view = view;
        }

        public void DashboaredData()
        {
            try
            {
                _view.LoadCategories(_repository.GetCategories());
                _view.LoadSuppliers(_repository.GetSuppliers());
            }
            catch (Exception ex)
            {
                _view.ShowMessage("Critical Error Loading Data: " + ex.Message);
            }
        }

        public void RefreshData()
        {
            try
            {
                var products = _repository.GetAll();
                _view.GridDataSource = products;
                UpdateCalculations(products);

                _view.LoadCategories(_repository.GetCategories());
                _view.LoadSuppliers(_repository.GetSuppliers());
            }
            catch (Exception ex)
            {
                _view.ShowError(ex.Message);
            }
        }

        public void Search(string keyword)
        {
            try
            {
                var products = _repository.Search(keyword);
                _view.GridDataSource = products;
                UpdateCalculations(products);
            }
            catch (Exception ex)
            {
                _view.ShowError(ex.Message);
            }
        }

        public void UpdateCalculations(IEnumerable<Product> products)
        {
            if (products == null || !products.Any())
            {
                _view.TotalItemsText = "Total Products: 0";
                _view.TotalValueText = "Total Value: $0.00";
                _view.TotalProfitText = "Total Potential Profit: $0.00";
                _view.LowStockText = "Low Stock Alerts: 0";
                return;
            }

            int totalItems = products.Count();
            decimal totalValue = 0;
            int lowStock = 0;
            decimal totalPotentialProfit = 0;

            foreach (var product in products)
            {
                totalValue += (product.Price * product.Quantity);
                totalPotentialProfit += ((product.Price - product.CostPrice) * product.Quantity);

                if (product.Quantity < 5)
                    lowStock++;
            }

            _view.TotalItemsText = $"Total Products: {totalItems}";
            _view.TotalValueText = $"Total Value: {totalValue:C}";
            _view.TotalProfitText = $"Total Potential Profit: {totalPotentialProfit:C}";
            _view.LowStockText = $"Low Stock Alerts: {lowStock}";
        }

        public void DeleteProduct(int id, string name)
        {
            if (_view.ConfirmDelete(name))
            {
                try
                {
                    _repository.Delete(id);
                    RefreshData();
                }
                catch (Exception ex)
                {
                    _view.ShowError(ex.Message);
                }
            }
        }

        public void SaveProduct()
        {

            if (UserSession.Role == UserRole.Cashier)
            {
                _view.ShowError("Security Violation: Cashiers are strictly forbidden from modifying master product records.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_view.ViewProductName))
            {
                _view.ShowMessage("Product Name is Required");
                return;
            }

            if (_view.CategoryId <= 0)
            {
                _view.ShowMessage("Please select a category.");
                return;
            }

            if (_view.ProductPrice <= 0)
            {
                _view.ShowMessage("Price must be greater than zero.");
                return;
            }

            try
            {
                bool isNewProduct = _view.ProductId == 0;
                string action = isNewProduct ? "Create" : "Edit";

                var product = new Product
                {
                    ProductId = _view.ProductId,
                    Name = _view.ViewProductName,
                    CategoryId = _view.CategoryId,
                    Quantity = _view.ProductQuantity,
                    Price = _view.ProductPrice,
                    CostPrice = _view.CostPrice,
                    SupplierId = _view.SupplierId
                };

                using (var scope = new TransactionScope())
                {
                    _repository.UpSert(product);


                    string changesMade = isNewProduct
                        ? $"Create new product '{product.Name}' with initial quantity {product.Quantity}."
                        : $"Updated product '{product.Name}'. price: {product.Price:C}, Cost: {product.CostPrice:C}.";


                    _repository.LogSystemAudit(
                        userId: UserSession.UserId,
                        entityType: "Product",
                        entityId: _view.ProductId,
                        action: action,
                        chnagesMade: changesMade
                    );

                    scope.Complete();
                }

                _view.ShowMessage("Product saved successfully.");
                RefreshData(); 
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message);
            }
        }

        public void MakeTransaction()
        {
            if (UserSession.Role == UserRole.Cashier && _view.TransactionType != TransactionAction.Sale.ToString())
            {
                _view.ShowError("Security Violation: Cashiers are restricted to outbound sales only.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_view.TransactionType))
            {
                _view.ShowMessage("Transaction Type is Required");
                return;
            }

            if (_view.ProductId <= 0)
            {
                _view.ShowMessage("Please select a Product.");
                return;
            }

            if (_view.SoldQty <= 0)
            {
                _view.ShowMessage("Sold quantity must be greater than zero.");
                return;
            }

            int finalQty = _view.TransactionType == "Sale" ? (_view.SoldQty * -1) : _view.SoldQty;
            decimal unitPrice = _view.TransactionType == "Sale" ? _view.ProductPrice : _view.CostPrice;
            decimal totalAmount = unitPrice * _view.SoldQty;

            decimal profit = _view.TransactionType == "Sale" ? totalAmount - (_view.CostPrice * _view.SoldQty) : 0;

            try
            {
                _repository.LogInventoryTransaction(
                    productId: _view.ProductId,
                    SupplierId: _view.SupplierId > 0 ? _view.SupplierId : null,
                    UserId: UserSession.UserId,
                    transactionType: _view.TransactionType,
                    quantity: finalQty,
                    unitPrice: unitPrice,
                    totalAmount: totalAmount,
                    profit: profit
                );

                _view.ShowMessage($"Transaction ({_view.TransactionType}) logged successfully.");
                RefreshData();
            }
            catch (Exception ex)
            {
                _view.ShowMessage("Transaction Failed: " + ex.Message);
            }
        }
    }
}

