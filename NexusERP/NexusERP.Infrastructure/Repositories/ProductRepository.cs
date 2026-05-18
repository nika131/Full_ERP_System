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
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace NexusERP.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public IEnumerable<Product> GetAll()
        {
            var query = from p in _context.Products.AsNoTracking()
                        join c in _context.Categories on p.CategoryId equals c.CategoryId
                        select new Product
                        {
                            ProductId = p.ProductId,
                            Name = p.Name,
                            CategoryId = p.CategoryId,
                            CategoryName = c.CategoryName, 
                            Quantity = p.Quantity,
                            Price = p.Price,
                            CostPrice = p.CostPrice,
                            SupplierId = p.SupplierId
                        };

            return query.ToList();
        }

        public IEnumerable<Product> Search(string keyword)
        {
            var query = from p in _context.Products.AsNoTracking()
                        join c in _context.Categories on p.CategoryId equals c.CategoryId
                        where p.Name.Contains(keyword) || p.ProductId.ToString().Contains(keyword)
                        select new Product
                        {
                            ProductId = p.ProductId,
                            Name = p.Name,
                            CategoryId = p.CategoryId,
                            CategoryName = c.CategoryName,
                            Quantity = p.Quantity,
                            Price = p.Price,
                            CostPrice = p.CostPrice,
                            SupplierId = p.SupplierId 
                        };

            return query.ToList();
        }


        public void UpSert(Product product)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_UpsertProduct @id, @name, @catId, @qty, @price, @costPrice, @supplierId",
                new SqlParameter("@id", product.ProductId),
                new SqlParameter("@name", product.Name),
                new SqlParameter("@catId", product.CategoryId),
                new SqlParameter("@qty", product.Quantity),
                new SqlParameter("@price", product.Price),
                new SqlParameter("@costPrice", product.CostPrice),
                new SqlParameter("@supplierId", product.SupplierId)
            );
        }

        public void LogInventoryTransaction(int productId, int? supplierId, int userId, string transactionType, int quantity, decimal unitPrice, decimal totalAmount, decimal profit)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_LogInventoryTransaction @ProductId, @SupplierId, @UserId, @TransactionType, @Quantity, @UnitPrice, @TotalAmount, @Profit",
                new SqlParameter("@ProductId", productId),
                new SqlParameter("@SupplierId", supplierId ?? (object)DBNull.Value),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@TransactionType", transactionType),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@UnitPrice", unitPrice),
                new SqlParameter("@TotalAmount", totalAmount),
                new SqlParameter("@Profit", profit)
            );
        }

        public void LogSystemAudit(int userId, string entityType, int entityId, string action, string changesMade)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_LogSystemAudit @UserId, @EntityType, @EntityId, @Action, @ChangesMade",
                new SqlParameter("@UserId", userId),
                new SqlParameter("@EntityType", entityType),
                new SqlParameter("@EntityId", entityId),
                new SqlParameter("@Action", action),
                new SqlParameter("@ChangesMade", changesMade)
            );
        }

        public void Delete(int id)
        {
            _context.Database.ExecuteSqlRaw("EXEC sp_DeleteProduct @productId", new SqlParameter("@productId", id));
        }

        public IEnumerable<Category> GetCategories()
        {
            return _context.Categories.AsNoTracking().ToList();
        }

        public IEnumerable<Supplier> GetSuppliers()
        {
            return _context.Suppliers.AsNoTracking().ToList();
        }
    }
}
