using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Domain.Models;
using NexusERP.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Upsert(Product product, int userId)
        {
            bool isNew = product.ProductId == 0;
            string action = isNew ? "Create" : "Edit";
            string changes = isNew ? $"Created product '{product.Name}'" : $"Updated product '{product.Name}'";

            if (isNew)
            {
                _context.Products.Add(product);
            } 
            else
            {
                var existing = _context.Products.Find(product.ProductId);
                if (existing == null) throw new AppException("Product not Found");

                existing.Name = product.Name;
                existing.CategoryId = product.CategoryId;
                existing.SupplierId = product.SupplierId;
                existing.Price = product.Price;
                existing.CostPrice = product.CostPrice;
            }

            var audit = new SystemAuditLog
            {
                UserId = userId,
                EntityType = "Product",
                EntityId = product.ProductId,
                Action = action,
                ChangesMade = changes,
            };

            _context.SystemAuditLogs.Add(audit);
            _context.SaveChanges();
        }

        public void LogInventoryTransaction(InventoryTransaction transaction, int userId, string transactionType)
        {
            transaction.Quantity = Math.Abs(transaction.Quantity);
            transaction.UserId = userId;

            var product = _context.Products.Find(transaction.ProductId);
            if (product == null) throw new AppException("Product not found.");
   
            if(transactionType == "Sale")
            {
                if (product.Quantity < transaction.Quantity)
                {
                    throw new AppException($"Insufficient stock. Only {product.Quantity} unitts available.");
                }

                product.Quantity -= transaction.Quantity;
            }
            else if (transactionType == "Restock" || transactionType == "Adjust")
            {
                product.Quantity += transaction.Quantity;
            }

            _context.InventoryTransactions.Add(transaction);

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AppException("Inventory was modified by another user. Please refresh and try again.");
            }
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

    }
}
