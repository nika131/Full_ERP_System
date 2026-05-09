using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product_Inventory_Manager.Views.Interfaces
{
    public interface IMainView
    {
        DataTable gridDataSource { set; }

        string totalItemsText { set; }
        string totalValueText { set; }
        string totalProfitText { set; }
        string lowStockText { set; }
        Color lowStockColor { set; }

        void showError(string message);
        bool confirmDelete(string productName);

        int productId { get; set; }
        string productName { get; set; }
        decimal productPrice { get; set; }
        int productQuantity { get; set; }
        int categoryId { get; set; }
        int initialCategoryId { get; set; }
        decimal costPrice { get; set; }
        int supplierId { get; set; }
        int soldQty { get; set; }
        string transactionType { get; set; }

        void showMessage(string message);
        void loadCategories(DataTable categories);

        void loadSuppliers(DataTable suppliers);
    }
}
