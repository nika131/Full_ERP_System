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
using NexusERP.Domain.Models;

namespace NexusERP.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public PagedResult<Product> GetPaged(int pageNumber, int pageSize, string? searchTerm)
        {
            var baseQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                baseQuery = baseQuery.Where(p => p.Name.Contains(searchTerm) || p.ProductId.ToString() == searchTerm);
            }

            var totalCount = baseQuery.Count();

            var items = baseQuery
                .OrderByDescending(p => p.ProductId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Product>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public void UpSert(Product product)
        {
            if(product.ProductId == 0)
            {
                product.Quantity = 0;
                _context.Products.Add(product);
            }
            else
            {
                var existing = _context.Products.Find(product.ProductId);
                if (existing == null) throw new Exception("Product not Found");

                existing.Name = product.Name;
                existing.CategoryId = product.CategoryId;
                existing.SupplierId = product.SupplierId;
                existing.Price = product.Price;
                existing.CostPrice = product.CostPrice;
                existing.UpdatedAt = DateTime.Now;
            }
            _context.SaveChanges();
        }

        public void LogInventoryTransaction(InventoryTransaction transaction, int userId)
        {
            transaction.UserId = userId;
            _context.InventoryTransactions.Add(transaction);

            var product = _context.Products.Find(transaction.ProductId);
            if (product != null)
            {
                product.Quantity += transaction.Quantity;
            }
            else
            {
                throw new Exception("Product not found.");
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
                ChangesMade = changesMade,
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
                product.IsActive = false;
                product.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public DashboardResponse GetDashboardAggregates()
        {
            var stats = _context.Products
                .GroupBy(p => 1)
                .Select(g => new DashboardResponse
                {
                    TotalValue = g.Sum(p => p.Price * p.Quantity),
                    TotalCost = g.Sum(p => p.CostPrice * p.Quantity),
                }).FirstOrDefault() ?? new DashboardResponse();

            stats.LowStockCount = _context.Products.Count(p => p.Quantity < 5);

            stats.TotalProfit = stats.TotalValue - stats.TotalCost;
            stats.MarginPrecentage = stats.TotalValue > 0 ? (stats.TotalProfit / stats.TotalValue) * 100 : 0;

            if (stats.MarginPrecentage > 30 && stats.LowStockCount == 0)
                stats.InventoryHealth = "EXCELLENT";
            else if (stats.LowStockCount > 0)
                stats.InventoryHealth = "ACTION REQUIRED";
            else
                stats.InventoryHealth = "STABLE";

            return stats;
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
