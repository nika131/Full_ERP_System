using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product_Inventory_Manager.Repositories
{
    public interface IProductRepository
    {
        DataTable getAll();
        DataTable search(string keyword);
        void upSert(int id, string name, int catId, int qty, decimal price, decimal costPrice, int supplierId);
        void makeTransaction(int productId, string transactionType, int qty);
        void delete(int id);
        DataTable getCategories();
        DataTable getSuppliers();
    }
}
