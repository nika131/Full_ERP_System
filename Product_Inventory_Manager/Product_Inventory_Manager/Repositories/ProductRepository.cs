using Product_Inventory_Manager.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Product_Inventory_Manager.Repositories
{
    internal class ProductRepository : IProductRepository
    {
        public DataTable getAll() => DatabaseHelper.ExecuteStoredProcedure("sp_GetAllProducts");

        public DataTable search(string keyword) =>
            DatabaseHelper.ExecuteStoredProcedure("sp_SearchProducts", new Dictionary<string, object> { { "@Keyword", keyword } });

        public void upSert(int id, string name, int catId, int qty, decimal price, decimal costPrice, int supplierId)
        {
            var args = new Dictionary<string, object>
            {
                { "@id", id }, 
                { "@name", name}, 
                { "@catId", catId }, 
                { "@qty", qty }, 
                { "@price", price },
                { "@costPrice", costPrice },
                { "@supplierId", supplierId }
            };
            DatabaseHelper.ExecuteNonQuery("sp_UpsertProduct", args);
        }

        public void makeTransaction(int productId, string transactionType, int qty)
        {
            var args = new Dictionary<string, object>
            {
                { "@productId", productId },
                { "@transactionType", transactionType },
                { "@qty", qty }
            };
            DatabaseHelper.ExecuteNonQuery("sp_MakeTransaction", args);
        }

        public void delete(int id) =>
            DatabaseHelper.ExecuteNonQuery("sp_DeleteProduct", new Dictionary<string, object> { { "@productId", id } });

        public DataTable getCategories() => DatabaseHelper.ExecuteStoredProcedure("sp_GetCategories");

        public DataTable getSuppliers() => DatabaseHelper.ExecuteStoredProcedure("sp_GetSuppliers");
    }
}
