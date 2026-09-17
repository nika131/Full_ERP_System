using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
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

        public async Task<(PagedResult<Product> Result, decimal totalValue)> GetPaged(int pageNumber, int pageSize, string? searchTerm, string? categoryName, string? supplierName, bool lowStockOnly = false)
        {
            var baseQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.IsActive)
                .AsNoTracking();

            if (lowStockOnly)
            {
                var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "GlobalLowStockThreshold");
                int globalThreshold = setting != null && int.TryParse(setting.SettingValue, out int parsed) ? parsed : 5;

                baseQuery = baseQuery.Where(p => p.Quantity <= (p.LowStockThreshold ?? globalThreshold));
            }

            if (!string.IsNullOrEmpty(supplierName))
            {
                baseQuery = baseQuery.Where(p =>
                    p.Supplier.ContactName == supplierName
                );
            }

            if (!string.IsNullOrEmpty(categoryName))
            {
                baseQuery = baseQuery.Where(p =>
                    p.Category.CategoryName == categoryName
                );
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                bool isNumeric = int.TryParse(searchTerm, out int searchId);
                baseQuery = baseQuery.Where(p => 
                    p.Name.Contains(searchTerm) || 
                    (isNumeric && p.ProductId == searchId)
                );
            }

            var totalCount = await baseQuery.CountAsync();
            var totalValue = await baseQuery.SumAsync(p => p.Price * p.Quantity);

            var items = await baseQuery
                .OrderByDescending(p => p.ProductId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedResult =  new PagedResult<Product>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return (pagedResult, totalValue);
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
                existing.LowStockThreshold = product.LowStockThreshold;
                existing.VatRate = product.VatRate;
                existing.MarketDiscountRate = product.MarketDiscountRate;
                existing.MaxDiscountPercentage = product.MaxDiscountPercentage;
                existing.Barcode = product.Barcode;
                existing.ImageUrl = product.ImageUrl;
                existing.ShapeType = product.ShapeType;
                existing.ShapeColor = product.ShapeColor;
                existing.ShapeText = product.ShapeText;
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

        public async Task SaveTransaction(InventoryTransaction transaction, Product product, SystemAuditLog audit)
        {
            _context.SystemAuditLogs.Add(audit);
            _context.InventoryTransactions.Add(transaction);
            _context.Products.Update(product); 
            await _context.SaveChangesAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .FirstAsync(p => id == p.ProductId);
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

        public async Task<DashboardResponse> GetDashboardAggregates(DashboardFilterRequest dto)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "GlobalLowStockThreshold");
            int globalDefaultThreshold = setting != null && int.TryParse(setting.SettingValue, out int parsed) ? parsed : 5;

            var productQuery = _context.Products.Where(p => p.IsActive).AsQueryable();

            if (dto.CategoryIds != null && dto.CategoryIds.Any()) productQuery = productQuery.Where(p => dto.CategoryIds.Contains(p.CategoryId));
            if (dto.SupplierIds != null && dto.SupplierIds.Any()) productQuery = productQuery.Where(p => dto.SupplierIds.Contains(p.SupplierId));

            var inventoryStats = await productQuery
                .GroupBy(p => 1)
                .Select(g => new 
                {
                    TotalValue = g.Sum(p => p.Price * p.Quantity),
                    TotalCost = g.Sum(p => p.CostPrice * p.Quantity),
                    lowStockCount = g.Count(p => p.Quantity <= (p.LowStockThreshold ?? globalDefaultThreshold))
                }).FirstOrDefaultAsync();

            var salesQuery = _context.ReceiptItems
                .Include(ri => ri.Receipt)
                .Include(ri => ri.Product)
                .Where(ri => ri.Receipt!.IsActive)
                .AsQueryable();

            if (dto.StartDate.HasValue) salesQuery = salesQuery.Where(ri => ri.Receipt!.CreatedAt >= dto.StartDate.Value);
            if (dto.EndDate.HasValue) 
            {
                var endOfDay = dto.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                salesQuery = salesQuery.Where(ri => ri.Receipt!.CreatedAt <= endOfDay);
            }

            if (dto.StartHour.HasValue) salesQuery = salesQuery.Where(ri => ri.Receipt!.CreatedAt.TimeOfDay >= dto.StartHour.Value);
            if (dto.EndHour.HasValue) salesQuery = salesQuery.Where(ri => ri.Receipt!.CreatedAt.TimeOfDay <= dto.EndHour.Value);

            if (dto.StoreIds != null && dto.StoreIds.Any()) salesQuery = salesQuery.Where(ri => dto.StoreIds.Contains(ri.Receipt!.StoreId));
            if (dto.CategoryIds != null && dto.CategoryIds.Any()) salesQuery = salesQuery.Where(ri => dto.CategoryIds.Contains(ri.Product!.CategoryId));
            if (dto.SupplierIds != null && dto.SupplierIds.Any()) salesQuery = salesQuery.Where(ri => dto.SupplierIds.Contains(ri.Product!.SupplierId));
            if (dto.EmployeeIds != null && dto.EmployeeIds.Any()) salesQuery = salesQuery.Where(ri => dto.EmployeeIds.Contains(ri.Receipt!.UserId));

            var totalSalesAmount = await salesQuery.SumAsync(ri => ri.LineTotal);
            var realizedProfit = await salesQuery.SumAsync(ri => ri.LineTotal - (ri.Product!.CostPrice * ri.Quantity));

            var stats = new DashboardResponse
            {
                TotalValue = inventoryStats?.TotalValue ?? 0,
                TotalCost = inventoryStats?.TotalCost ?? 0,
                LowStockCount = inventoryStats?.lowStockCount ?? 0,
                TotalProfit = realizedProfit,
                TotalSales = totalSalesAmount,
                MarginPrecentage = (inventoryStats?.TotalValue ?? 0) > 0
                    ? (realizedProfit / inventoryStats!.TotalValue) * 100
                    : 0
            };

            if (stats.MarginPrecentage > 30 && stats.LowStockCount == 0)
                stats.InventoryHealth = "EXCELLENT";
            else if (stats.LowStockCount > 0)
                stats.InventoryHealth = "ACTION REQUIRED";
            else
                stats.InventoryHealth = "STABLE";

            return stats;
        }

        public async Task<List<Product>> GetProductsLowOnStock()
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "GlobalLowStockThreshold");
            int globalDefaultThreshold = setting != null && int.TryParse(setting.SettingValue, out int parsed) ? parsed : 5; 

            return await _context.Products
                .Where(p => p.Quantity <= (p.LowStockThreshold ?? globalDefaultThreshold))
                .ToListAsync();
        }
    }
}
