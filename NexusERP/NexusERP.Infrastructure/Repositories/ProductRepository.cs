using Microsoft.EntityFrameworkCore;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Exceptions;
using NexusERP.Domain.Models;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(PagedResult<Product> Result, decimal totalValue)> GetPaged(int companyId, int pageNumber, int pageSize, string? searchTerm, string? categoryName, string? supplierName, bool lowStockOnly = false, List<int>? excludeIds = null)
        {
            var baseQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.CompanyId == companyId)
                .AsNoTracking();

            if (excludeIds != null && excludeIds.Count > 0)
                baseQuery = baseQuery.Where(p => !excludeIds.Contains(p.ProductId));

            if (lowStockOnly)
            {
                var setting = await _context.SystemSettings.FirstOrDefaultAsync(s =>
                    s.CompanyId == companyId &&
                    s.SettingKey == "GlobalLowStockThreshold"
                );

                int globalThreshold = setting != null && int.TryParse(setting.SettingValue, out int parsed) ? parsed : 5;

                baseQuery = baseQuery.Where(p => p.Quantity <= (p.LowStockThreshold ?? globalThreshold));
            }

            if (!string.IsNullOrEmpty(supplierName))
                baseQuery = baseQuery.Where(p => p.Supplier != null && p.Supplier.CompanyName == supplierName);

            if (!string.IsNullOrEmpty(categoryName))
                baseQuery = baseQuery.Where(p => p.Category != null && p.Category.CategoryName == categoryName);

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

            var pagedResult = new PagedResult<Product>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return (pagedResult, totalValue);
        }

        public async Task Upsert(Product product, int userId, int companyId)
        {
            await ValidateProductRelationsAsync(product, companyId);

            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    bool isNew = product.ProductId == 0;
                    Product targetProduct;

                    if (isNew)
                    {
                        product.CompanyId = companyId;
                        product.IsActive = true;

                        _context.Products.Add(product);
                        targetProduct = product;
                    }
                    else
                    {
                        var existing = await _context.Products.FirstOrDefaultAsync(p =>
                            p.ProductId == product.ProductId &&
                            p.CompanyId == companyId
                        );

                        if (existing == null)
                            throw new AppException("Product not found.");

                        if (!string.IsNullOrEmpty(existing.ImageUrl) && existing.ImageUrl != product.ImageUrl)
                        {
                            var oldFilePath = Path.Combine(
                                Directory.GetCurrentDirectory(),
                                "wwwroot",
                                existing.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                            );

                            if (System.IO.File.Exists(oldFilePath))
                                System.IO.File.Delete(oldFilePath);
                        }

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
                        existing.DisplayMode = product.DisplayMode;

                        targetProduct = existing;
                    }

                    await _context.SaveChangesAsync();

                    var audit = new SystemAuditLog
                    {
                        CompanyId = companyId,
                        UserId = userId,
                        EntityType = "Product",
                        EntityId = targetProduct.ProductId,
                        Action = isNew ? "Create" : "Edit",
                        ChangesMade = isNew
                            ? $"Created product '{targetProduct.Name}'"
                            : $"Updated product '{targetProduct.Name}'"
                    };

                    _context.SystemAuditLogs.Add(audit);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task SaveTransaction(InventoryTransaction transaction, Product product, SystemAuditLog audit, int companyId)
        {
            if (product.CompanyId != companyId)
                throw new AppException("Product does not belong to this company.");

            transaction.CompanyId = companyId;
            audit.CompanyId = companyId;

            _context.SystemAuditLogs.Add(audit);
            _context.InventoryTransactions.Add(transaction);
            _context.Products.Update(product);

            await _context.SaveChangesAsync();
        }

        public async Task<Product> GetByIdAsync(int companyId, int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p =>
                p.ProductId == id &&
                p.CompanyId == companyId
            );

            if (product == null)
                throw new AppException("Product not found.");

            return product;
        }

        public async Task Delete(int productId, int userId, int companyId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p =>
                p.ProductId == productId &&
                p.CompanyId == companyId
            );

            if (product == null)
                throw new AppException("Product not found.");

            product.IsActive = false;

            var audit = new SystemAuditLog
            {
                CompanyId = companyId,
                UserId = userId,
                EntityType = "Product",
                EntityId = product.ProductId,
                Action = "Delete",
                ChangesMade = $"Deleted product '{product.Name}'"
            };

            await _context.SystemAuditLogs.AddAsync(audit);
            await _context.SaveChangesAsync();
        }

        public async Task<DashboardResponse> GetDashboardAggregates(int companyId, DashboardFilterRequest dto)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s =>
                s.CompanyId == companyId &&
                s.SettingKey == "GlobalLowStockThreshold"
            );

            int globalDefaultThreshold = setting != null && int.TryParse(setting.SettingValue, out int parsed) ? parsed : 5;

            var productQuery = _context.Products
                .Where(p => p.CompanyId == companyId)
                .AsQueryable();

            if (dto.CategoryIds != null && dto.CategoryIds.Any())
                productQuery = productQuery.Where(p => p.CategoryId.HasValue && dto.CategoryIds.Contains(p.CategoryId.Value));

            if (dto.SupplierIds != null && dto.SupplierIds.Any())
                productQuery = productQuery.Where(p => p.SupplierId.HasValue && dto.SupplierIds.Contains(p.SupplierId.Value));

            var inventoryStats = await productQuery
                .GroupBy(p => 1)
                .Select(g => new
                {
                    TotalValue = g.Sum(p => p.Price * p.Quantity),
                    TotalCost = g.Sum(p => p.CostPrice * p.Quantity),
                    LowStockCount = g.Count(p => p.Quantity <= (p.LowStockThreshold ?? globalDefaultThreshold))
                })
                .FirstOrDefaultAsync();

            var salesQuery = _context.ReceiptItems
                .Include(ri => ri.Receipt)
                .Include(ri => ri.Product)
                .Where(ri =>
                    ri.CompanyId == companyId &&
                    ri.Receipt != null &&
                    ri.Receipt.CompanyId == companyId &&
                    ri.Receipt.IsActive
                )
                .AsQueryable();

            if (dto.StartDate.HasValue)
                salesQuery = salesQuery.Where(ri => ri.Receipt!.CreatedAt >= dto.StartDate.Value);

            if (dto.EndDate.HasValue)
            {
                var endOfDay = dto.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                salesQuery = salesQuery.Where(ri => ri.Receipt!.CreatedAt <= endOfDay);
            }

            if (dto.StartHour.HasValue)
                salesQuery = salesQuery.Where(ri => ri.Receipt!.CreatedAt.TimeOfDay >= dto.StartHour.Value);

            if (dto.EndHour.HasValue)
                salesQuery = salesQuery.Where(ri => ri.Receipt!.CreatedAt.TimeOfDay <= dto.EndHour.Value);

            if (dto.StoreIds != null && dto.StoreIds.Any())
                salesQuery = salesQuery.Where(ri => dto.StoreIds.Contains(ri.Receipt!.StoreId));

            if (dto.CategoryIds != null && dto.CategoryIds.Any())
                salesQuery = salesQuery.Where(ri => ri.Product!.CategoryId.HasValue && dto.CategoryIds.Contains(ri.Product.CategoryId.Value));

            if (dto.SupplierIds != null && dto.SupplierIds.Any())
                salesQuery = salesQuery.Where(ri => ri.Product!.SupplierId.HasValue && dto.SupplierIds.Contains(ri.Product.SupplierId.Value));

            if (dto.EmployeeIds != null && dto.EmployeeIds.Any())
                salesQuery = salesQuery.Where(ri => dto.EmployeeIds.Contains(ri.Receipt!.UserId));

            var totalSalesAmount = await salesQuery.SumAsync(ri => ri.LineTotal);
            var realizedProfit = await salesQuery.SumAsync(ri => ri.LineTotal - (ri.Product!.CostPrice * ri.Quantity));

            var stats = new DashboardResponse
            {
                TotalValue = inventoryStats?.TotalValue ?? 0,
                TotalCost = inventoryStats?.TotalCost ?? 0,
                LowStockCount = inventoryStats?.LowStockCount ?? 0,
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

        public async Task<List<Product>> GetProductsLowOnStock(int companyId)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s =>
                s.CompanyId == companyId &&
                s.SettingKey == "GlobalLowStockThreshold"
            );

            int globalDefaultThreshold = setting != null && int.TryParse(setting.SettingValue, out int parsed) ? parsed : 5;

            return await _context.Products
                .Where(p =>
                    p.CompanyId == companyId &&
                    p.Quantity <= (p.LowStockThreshold ?? globalDefaultThreshold)
                )
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> IsNegativeInventoryAllowedAsync(int companyId)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s =>
                s.CompanyId == companyId &&
                s.SettingKey == "AllowNegativeInventory"
            );

            return setting != null &&
                   bool.TryParse(setting.SettingValue, out bool parsed) &&
                   parsed;
        }

        private async Task ValidateProductRelationsAsync(Product product, int companyId)
        {
            if (product.CategoryId.HasValue)
            {
                bool categoryExists = await _context.Categories.AnyAsync(c =>
                    c.CategoryId == product.CategoryId.Value &&
                    c.CompanyId == companyId
                );

                if (!categoryExists)
                    throw new AppException("Selected category does not belong to this company.");
            }

            if (product.SupplierId.HasValue)
            {
                bool supplierExists = await _context.Suppliers.AnyAsync(s =>
                    s.SupplierId == product.SupplierId.Value &&
                    s.CompanyId == companyId
                );

                if (!supplierExists)
                    throw new AppException("Selected supplier does not belong to this company.");
            }
        }
    }
}