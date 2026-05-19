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
                            CategoryId = c.CategoryId,
                            CategoryName = c.CategoryName,
                            Quantity = p.Quantity,
                            Price = p.Price,
                            CostPrice = p.CostPrice,
                            SupplierId = p.SupplierId,
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
                            CategoryName = p.CategoryName,
                            Quantity = p.Quantity,
                            Price = p.Price,
                            CostPrice = p.CostPrice,
                            SupplierId = p.SupplierId,
                        };

            return query.ToList();
        }

        public void UpSert(Product product)
        {
            if(product.ProductId == 0)
            {
                _context.Products.Add(product);
            }
            else
            {
                _context.Products.Update(product);
            }
            _context.SaveChanges();
        }

        public void LogInventoryTransaction(int productId, int? supplierId, int UserId, string transactionType, int quantity, decimal unitPrice, decimal totalAmount, decimal profit)
        {
            var trasnsaction = new InventoryTransaction
            {
                ProductId = productId,
                SupplierId = supplierId,
                UserId = UserId,
                TransactionType = Enum.Parse<Domain.Enums.TransactionAction>(transactionType),
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalAmount = totalAmount,
                Profit = profit,
                CreatedAt = DateTime.Now
            };

            _context.InventoryTransactions.Add(trasnsaction);

            var product = _context.Products.Find(productId);

            if(product != null)
            {
                product.Quantity += quantity;
            }
            else
            {
                throw new Exception("Product not found. Cannot log transaction.");
            }

            _context.SaveChanges(); 
        }

        public void LogSystemAudit(int userId, string entityType, int entityId, string action, string changesMade)
        {
            var auditLog = new SystemAuditLog
            {
                UserId = userId,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                ChangeMade = changesMade,
                CreatedAt = DateTime.Now,
            };

            _context.SystemAuditLogs.Add(auditLog);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var product = _context.Products.Find(id);

            if(product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
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
