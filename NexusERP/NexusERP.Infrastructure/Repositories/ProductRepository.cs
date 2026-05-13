using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        public IEnumerable<Product> GetAll()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAllProducts");
            var products = new List<Product>();

            foreach (DataRow row in dt.Rows)
            {
                products.Add(new Product
                {
                    ProductId = Convert.ToInt32(row["ProductId"]),
                    ProductName = row["ProductName"] == DBNull.Value ? string.Empty : row["ProductName"].ToString()!,
                    ProductCategoryId = row["CategoryId"] == DBNull.Value ? 0 : Convert.ToInt32(row["CategoryId"]),
                    Category = row["Category"] == DBNull.Value ? string.Empty : row["Category"].ToString()!,
                    Quantity = row["Quantity"] == DBNull.Value ? 0 : Convert.ToInt32(row["Quantity"]),
                    ProductPrice = row["ProductPrice"] == DBNull.Value ? 0m : Convert.ToDecimal(row["ProductPrice"]),
                    ProductCostPrice = row["CostPrice"] == DBNull.Value ? 0m : Convert.ToDecimal(row["CostPrice"]),
                    SupplierId = row["SupplierId"] == DBNull.Value ? 0 : Convert.ToInt32(row["SupplierId"])
                });
            }
            return products;
        }

        public IEnumerable<Product> Search(string keyword)
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_SearchProducts", new Dictionary<string, object> { { "@Keyword", keyword } });
            var products = new List<Product>();
        
            foreach (DataRow row in dt.Rows)
            {
                products.Add(new Product
                {
                    ProductId = Convert.ToInt32(row["ProductId"]),
                    ProductName = row["ProductName"] == DBNull.Value ? string.Empty : row["ProductName"].ToString()!,
                    ProductCategoryId = row["CategoryId"] == DBNull.Value ? 0 : Convert.ToInt32(row["CategoryId"]),
                    Quantity = row["Quantity"] == DBNull.Value ? 0 : Convert.ToInt32(row["Quantity"]),
                    ProductPrice = row["ProductPrice"] == DBNull.Value ? 0m : Convert.ToDecimal(row["ProductPrice"]),
                    ProductCostPrice = row["CostPrice"] == DBNull.Value ? 0m : Convert.ToDecimal(row["CostPrice"]),
                    SupplierId = row["SupplierId"] == DBNull.Value ? 0 : Convert.ToInt32(row["SupplierId"])
                });
            }
            return products;
        }
            

        public void UpSert(Product product)
        {
            var args = new Dictionary<string, object>
            {
                { "@id", product.ProductId },
                { "@name", product.ProductName },
                { "@catId", product.ProductCategoryId },
                { "@qty", product.Quantity },
                { "@price", product.ProductPrice },
                { "@costPrice", product.ProductCostPrice },
                { "@supplierId", product.SupplierId }
            };
            DatabaseHelper.ExecuteNonQuery("sp_UpsertProduct", args);
        }

        public void MakeTransaction(int productId, int SupplierId, string transactionType, int qty, decimal Amount, decimal profit)
        {
            var args = new Dictionary<string, object>
            {
                { "@productId", productId },
                { "@SupplierId",  SupplierId },
                { "@transactionType", transactionType },
                { "@qty", qty },
                { "@Amount", Amount },
                { "@profit",  profit }
            };
            DatabaseHelper.ExecuteNonQuery("sp_MakeTransaction", args);
        }

        public void Delete(int id)
        {
            DatabaseHelper.ExecuteNonQuery("sp_DeleteProduct", new Dictionary<string, object> { { "@productId", id } });
        }


        public IEnumerable<Category> GetCategories()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetCategories");
            var categories = new List<Category>();

            foreach (DataRow row in dt.Rows)
            {
                categories.Add(new Category
                {
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    CategoryName = row["Name"] == DBNull.Value ? string.Empty : row["Name"].ToString()!
                });
            }
            return categories;
        }

        public IEnumerable<Supplier> GetSuppliers()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetSuppliers");
            var suppliers = new List<Supplier>();

            foreach (DataRow row in dt.Rows)
            {
                suppliers.Add(new Supplier
                {
                    SupplierId = Convert.ToInt32(row["SupplierId"]),
                    CompanyName = row["CompanyName"] == DBNull.Value ? string.Empty : row["CompanyName"].ToString()!
                });
            }
            return suppliers;
        }
    }
}
