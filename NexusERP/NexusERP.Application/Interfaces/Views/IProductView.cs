using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Views
{
    public interface IProductView
    {
        IEnumerable<Product> GridDataSource { set; }

        string TotalItemsText { set; }
        string TotalValueText { set; }
        string TotalProfitText { set; }
        string LowStockText { set; }
        int ProductId { get; set; }
        string ViewProductName { get; set; }
        decimal ProductPrice { get; set; }
        int ProductQuantity { get; set; }
        int CategoryId { get; set; }
        int InitialCategoryId { get; set; }
        decimal CostPrice { get; set; }
        int SupplierId { get; set; }
        int SoldQty { get; set; }
        string TransactionType { get; set; }

        void ShowError(string message);
        bool ConfirmDelete(string productName);
        void ShowMessage(string message);
        void LoadCategories(IEnumerable<Category> categories);
        void LoadSuppliers(IEnumerable<Supplier> suppliers);
    }
}
