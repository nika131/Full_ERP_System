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

        public async Task<PagedResult<InventoryTransaction>> GetPagedTransactions(int pageNumber, int pageSize, string? searchTerm, int currentUserId, bool canViewAll, string typeFilter)
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

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                baseQuery = baseQuery.Where(t =>
                    (t.Supplier != null && t.Supplier.CompanyName.Contains(searchTerm)) ||
                    (t.Product != null && t.Product.Name.Contains(searchTerm)) ||
                    t.TransactionId.ToString().Contains(searchTerm)
                );
            }

            var totalCount = await baseQuery.CountAsync();

            var items = await baseQuery
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            return new PagedResult<InventoryTransaction>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<InventoryTransaction?> GetById(int transactionId)
        {
            return await _context.InventoryTransactions.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }

        public async Task<List<RevenueChartData>> GetWeeklyRevenueChart()
        {
            var startDate = DateTime.Now.Date.AddDays(-6);

            var rawData = await _context.InventoryTransactions
                .Where(t => t.TransactionType == TransactionAction.Sale && t.CreatedAt >= startDate)
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(t => t.TotalAmount),
                    Profit = g.Sum(t => t.Profit)
                }).ToListAsync();

            var chartData = new List<RevenueChartData>();
            for(int i = 0; i <= 6; i++)
            {
                var targetDate = startDate.AddDays(i);
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

        public async Task<List<TopProductChartData>> GetTopPerformingProducts()
        {
            return await _context.InventoryTransactions
                .Include(t => t.Product)
                .Where(t => t.TransactionType == TransactionAction.Sale)
                .GroupBy(t => t.Product.Name)
                .Select(g => new TopProductChartData
                {
                    ProductName = g.Key ?? "Unknown",
                    Revenue = g.Sum(t => t.TotalAmount)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();
        }
    }
}
