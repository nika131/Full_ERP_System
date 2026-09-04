using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
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
    public class ReportRepository : IReportRepository
    {

        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CursorPagedResult<InventoryTransaction>> GetPagedTransactionsOptimized(
            int pageSize,
            int currentUserId,
            bool canViewAll,
            string typeFilter,
            string? searchTerm,
            DateTime? lastCreatedAt,
            int? lastTransactionId,
            int? productId,
            int? supplierId,
            int? storeId = null,
            int? categoryId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var baseQuery = _context.InventoryTransactions
                            .Include(t => t.Product)
                            .Include(t => t.Supplier)
                            .Include(t => t.User)
                            .AsNoTracking();

            if (!canViewAll)
            {
                baseQuery = baseQuery.Where(t => t.UserId == currentUserId);
            }


            if (typeFilter != "All" && Enum.TryParse<TransactionAction>(typeFilter, true, out var action))
            {
                baseQuery = baseQuery.Where(t => t.TransactionType == action);
            }


            if (productId.HasValue)
            {
                baseQuery = baseQuery.Where(t => t.ProductId == productId.Value);
            }

            if (supplierId.HasValue)
            {
                baseQuery = baseQuery.Where(t => t.SupplierId == supplierId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                if (int.TryParse(searchTerm, out int numericSearchId))
                {
                    baseQuery = baseQuery.Where(t => t.TransactionId == numericSearchId);
                }
                else
                {
                    baseQuery = baseQuery.Where(t => t.Product != null && t.Product.Name.Contains(searchTerm));
                }
            }

            if (storeId.HasValue)
            {
                baseQuery = baseQuery.Where(t => t.StoreId == storeId.Value);
            }

            if (categoryId.HasValue)
            {
                baseQuery = baseQuery.Where(t => t.Product != null && t.Product.CategoryId == categoryId.Value);
            }

            if (startDate.HasValue)
            {
                baseQuery = baseQuery.Where(t => t.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                baseQuery = baseQuery.Where(t => t.CreatedAt <= endOfDay);
            }


            if (lastCreatedAt.HasValue && lastTransactionId.HasValue)
            {
                baseQuery = baseQuery.Where(t =>
                    t.CreatedAt < lastCreatedAt.Value ||
                    (t.CreatedAt == lastCreatedAt.Value && t.TransactionId < lastTransactionId.Value));
            }

            int fetchCount = pageSize + 1;

            var items = await baseQuery 
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.TransactionId)
                .Take(fetchCount)
                .ToListAsync();

            bool hasMorePages = items.Count == fetchCount;

            DateTime? nextCreatedAtCursor = null;
            int? nextTransactionIdCursor = null;

            if (hasMorePages)
            {
                var lastValidItem = items[pageSize - 1];
                nextCreatedAtCursor = lastValidItem.CreatedAt;
                nextTransactionIdCursor = lastValidItem.TransactionId;

                items.RemoveAt(items.Count - 1);
            }

            return new CursorPagedResult<InventoryTransaction>
            {
                Items = items,
                NextCreatedAt = nextCreatedAtCursor,
                NextId = nextTransactionIdCursor,
                PageSize = pageSize
            };
        }

        public async Task<InventoryTransaction?> GetById(int transactionId)
        {
            return await _context.InventoryTransactions.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }

        public async Task<List<RevenueChartData>> GetWeeklyRevenueChart(DateTime? startDate, DateTime? endDate, int? storeId, int? categoryId, int? supplierId)
        {
            var actualStart = startDate ?? DateTime.UtcNow.Date.AddDays(-6);
            var actualEnd = endDate ?? DateTime.UtcNow.Date;

            var endOfDay = actualEnd.Date.AddDays(1).AddTicks(-1);

            var query = _context.InventoryTransactions
                .Include(t => t.Product)
                .Where(t => t.TransactionType == TransactionAction.Sale && t.CreatedAt >= actualStart && t.CreatedAt <= endOfDay)
                .AsQueryable();

            if (storeId.HasValue) query = query.Where(t => t.StoreId == storeId.Value);
            if (categoryId.HasValue) query = query.Where(t => t.Product!.CategoryId == categoryId.Value);
            if (supplierId.HasValue) query = query.Where(t => t.Product!.SupplierId == supplierId.Value);

            var rawData = await query
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(t => t.TotalAmount),
                    Profit = g.Sum(t => t.Profit)
                }).ToListAsync();

            var chartData = new List<RevenueChartData>();
            int totalDays = (int)(actualEnd.Date - actualStart.Date).TotalDays;

            for (int i = 0; i <= totalDays; i++)
            {
                var targetDate = actualStart.Date.AddDays(i);
                var dayData = rawData.FirstOrDefault(d => d.Date == targetDate);

                chartData.Add(new RevenueChartData
                {
                    Date = targetDate.ToString("MMM dd"),
                    Revenue = dayData?.Revenue ?? 0,
                    Profit = dayData?.Profit ?? 0
                });
            }

            return chartData;
        }

        public async Task<List<TopProductChartData>> GetTopPerformingProducts(DateTime? startDate, DateTime? endDate, int? storeId, int? categoryId, int? supplierId)
        {
            var query = _context.InventoryTransactions
                .Include(t => t.Product)
                .Where(t => t.TransactionType == TransactionAction.Sale)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(t => t.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.CreatedAt <= endOfDay);
            }

            if (storeId.HasValue) query = query.Where(t => t.StoreId == storeId.Value);
            if (categoryId.HasValue) query = query.Where(t => t.Product!.CategoryId == categoryId.Value);
            if (supplierId.HasValue) query = query.Where(t => t.Product!.SupplierId == supplierId.Value);

            return await query
                .GroupBy(t => new { t.ProductId, t.Product!.Name })
                .Select(g => new TopProductChartData
                {
                    ProductName = g.Key.Name ?? "Unknown",
                    Revenue = g.Sum(t => t.TotalAmount)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
