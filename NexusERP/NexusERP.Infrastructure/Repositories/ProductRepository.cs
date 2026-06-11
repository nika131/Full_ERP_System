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

        public async Task<PagedResult<Product>> GetPaged(int pageNumber, int pageSize, string? searchTerm)
        {
            var baseQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                baseQuery = baseQuery.Where(p => p.Name.Contains(searchTerm) || p.ProductId.ToString() == searchTerm);
            }

            var totalCount = await baseQuery.CountAsync();

            var items = await baseQuery
                .OrderByDescending(p => p.ProductId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task Upsert(Product product, int userId)
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
                var existing = await _context.Products.FindAsync(product.ProductId);
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
                CreatedAt = DateTime.UtcNow
            };

            _context.SystemAuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }

        public async Task LogInventoryTransaction(InventoryTransaction transaction, int userId, string transactionType)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                var qty = Math.Abs(transaction.Quantity);
                transaction.Quantity = qty;
                transaction.UserId = userId;

                var product = await _context.Products.FindAsync(transaction.ProductId);
                if (product == null) throw new AppException("Product not found.");

                switch (transactionType)
                {
                    case "Sale":
                        if (product.Quantity < qty) throw new AppException($"Insufficient stock. Only {product.Quantity} available.");
                        product.Quantity -= qty;
                        break;

                    case "Loss": 
                    case "Damage":
                        if (product.Quantity < qty) throw new AppException($"Cannot deduct {qty}. Only {product.Quantity} available.");
                        product.Quantity -= qty;
                        transaction.Profit = -(product.CostPrice * qty);
                        break;

                    case "Restock":
                        product.Quantity += qty;
                        break;

                    default:
                        throw new AppException("Invalid transaction type.");
                }

                await _context.InventoryTransactions.AddAsync(transaction);
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await tran.RollbackAsync();
                throw new AppException("Inventory was modified by another user. Please refresh and try again.");
            }
            catch
            {
                await tran.RollbackAsync();
                throw; 
            }

        }

        public async Task Delete(int productId, int userId)
        {
            var product = await _context.Products.FindAsync(productId);

            if(product != null)
            {
                product.IsActive = false;

                var audit = new SystemAuditLog
                {
                    UserId = userId,
                    EntityType = "Product",
                    EntityId = product.ProductId,
                    Action = "Delete",
                    ChangesMade = $"Deleted product '{product.Name}'"
                };

                await _context.SystemAuditLogs.AddAsync(audit);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<DashboardResponse> GetDashboardAggregates()
        {
            var stats = await _context.Products
                .GroupBy(p => 1)
                .Select(g => new DashboardResponse
                {
                    TotalValue = g.Sum(p => p.Price * p.Quantity),
                    TotalCost = g.Sum(p => p.CostPrice * p.Quantity),
                }).FirstOrDefaultAsync() ?? new DashboardResponse();

            stats.LowStockCount = await _context.Products.CountAsync(p => p.Quantity < 5);

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
