using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NexusERP.Application.DTOs;
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
                                .ThenInclude(p => p!.Supplier)
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
                baseQuery = baseQuery.Where(t => t.Product != null && t.Product.SupplierId == supplierId.Value);
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

            var query = _context.ReceiptItems
                .Include(ri => ri.Receipt)
                .Include(ri => ri.Product)
                .Where(ri => ri.Receipt!.IsActive && ri.Receipt.CreatedAt >= actualStart && ri.Receipt.CreatedAt <= endOfDay)
                .AsQueryable();

            if (storeId.HasValue) query = query.Where(ri => ri.Receipt!.StoreId == storeId.Value);
            if (categoryId.HasValue) query = query.Where(ri => ri.Product!.CategoryId == categoryId.Value);
            if (supplierId.HasValue) query = query.Where(ri => ri.Product!.SupplierId == supplierId.Value);

            var rawData = await query
                .GroupBy(ri => ri.Receipt!.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(ri => ri.LineTotal),
                    Profit = g.Sum(ri => ri.LineTotal - (ri.Product!.CostPrice * ri.Quantity))
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
            var query = _context.ReceiptItems
                .Include(ri => ri.Receipt)
                .Include(ri => ri.Product)
                .Where(ri => ri.Receipt!.IsActive)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(ri => ri.Receipt!.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(ri => ri.Receipt!.CreatedAt <= endOfDay);
            }

            if (storeId.HasValue) query = query.Where(ri => ri.Receipt!.StoreId == storeId.Value);
            if (categoryId.HasValue) query = query.Where(ri => ri.Product!.CategoryId == categoryId.Value);
            if (supplierId.HasValue) query = query.Where(ri => ri.Product!.SupplierId == supplierId.Value);

            return await query
                .GroupBy(ri => new { ri.ProductId, ri.Product!.Name })
                .Select(g => new TopProductChartData
                {
                    ProductName = g.Key.Name ?? "Unknown",
                    Revenue = g.Sum(ri => ri.LineTotal)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PagedResult<ShiftAuditDto>> GetPagedShiftsAsync(int PageNumber, int pageSize, int? storeId)
        {
            var query = _context.Shifts
                .Include(s => s.User)
                .Include(s => s.Store)
                .AsNoTracking()
                .AsQueryable();

            if (storeId.HasValue) query = query.Where(s => s.StoreId == storeId.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(s => s.StartDate)
                .Skip((PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ShiftAuditDto
                {
                    ShiftId = s.ShiftId,
                    StoreName = s.Store!.Name,
                    CashierName = s.User!.Username,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    StartingCash = s.StartingCash,
                    ExpectedEndingCash = s.ExpectedEndingCash,
                    ActualEndingCash = s.ActualEndingCash,
                    TotalSales = s.TotalSales,
                    Status = s.Status.ToString(),
                }).ToListAsync();

            return new PagedResult<ShiftAuditDto> { Items = items, TotalCount = totalCount, PageNumber = PageNumber, PageSize = pageSize};
        }

        public async Task<PagedResult<ReceiptAuditDto>> GetPagedReceiptsAsync(int pageNumber, int pageSize, string? receiptNumber)
        {
            var query = _context.Receipts
                .Include(r => r.User)
                .Include(r => r.Store)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(receiptNumber))
                query = query.Where(r => r.ReceiptNumber.Contains(receiptNumber));

            var totalcount = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReceiptAuditDto
                {
                    ReceiptId = r.ReceiptId,
                    ReceiptNumber = r.ReceiptNumber,
                    StoreName = r.Store!.Name,
                    CashierName = r.User!.Username,
                    CreatedAt = r.CreatedAt,
                    SubTotal = r.SubTotal,
                    CartDiscountAmount = r.CartDiscountAmount,
                    TotalVatAmount = r.TotalVatAmount,
                    FinalTotal = r.FinalTotal,
                    PaymentMethod = r.PaymentMethod.ToString(),
                }).ToListAsync();

            return new PagedResult<ReceiptAuditDto> { Items = items, TotalCount = totalcount, PageNumber = pageNumber, PageSize = pageSize };
        }
    }
}
