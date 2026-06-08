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

        public PagedResult<InventoryTransaction> GetPagedTransactions(int pageNumber, int pageSize, String? searchterm, int currentUserId, string currentUserRole, string typeFilter)
        {
            var baseQuery = from t in _context.InventoryTransactions.AsNoTracking()
                            join p in _context.Products on t.ProductId equals p.ProductId into pJoin
                            from p in pJoin.DefaultIfEmpty()
                            join s in _context.Suppliers on t.SupplierId equals s.SupplierId into sJoin
                            from s in sJoin.DefaultIfEmpty()
                            join u in _context.Users.AsNoTracking() on t.UserId equals u.UserId into uJoin
                            from u in uJoin.DefaultIfEmpty()
                            select new { t, p, s, u };

            if (currentUserRole == "Admin")
            {
                
            }
            else if (currentUserRole == "Manager")
            {
                baseQuery = baseQuery.Where(item =>
                    item.t.UserId == currentUserId ||
                    (item.u != null && item.u.Role == NexusERP.Domain.Enums.UserRole.Cashier)
                );
            }
            else if (currentUserRole == "Cashier")
            {
                baseQuery = baseQuery.Where(item =>
                    item.t.TransactionType == NexusERP.Domain.Enums.TransactionAction.Sale &&
                    item.t.UserId == currentUserId
                );
            }
            else
            {
                baseQuery = baseQuery.Where(item => false);
            }

            if (typeFilter != "All" && Enum.TryParse(typeof(TransactionAction), typeFilter, true, out var actionObj))
            {
                var action = (TransactionAction)actionObj;
                baseQuery = baseQuery.Where(item => item.t.TransactionType == action);
            }

            if (!string.IsNullOrWhiteSpace(searchterm))
            {
                baseQuery = baseQuery.Where(item => 
                    (item.s.CompanyName != null && item.s.CompanyName.Contains(searchterm)) ||
                    (item.p.Name != null && item.p.Name.Contains(searchterm)) ||
                    item.t.TransactionId.ToString().Contains(searchterm));
            }

            var totalCount = baseQuery.Count();

            var items = baseQuery
                .OrderByDescending(item => item.t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(item => new InventoryTransaction
                {
                    TransactionId = item.t.TransactionId,
                    ProductId = item.t.ProductId,
                    SupplierId = item.t.SupplierId,
                    UserId = item.t.UserId,
                    TransactionType = item.t.TransactionType,
                    Quantity = item.t.Quantity,
                    UnitPrice = item.t.UnitPrice,
                    TotalAmount = item.t.TotalAmount,
                    Profit = item.t.Profit,
                    CreatedAt = item.t.CreatedAt,
                    ProductName = item.p != null ? item.p.Name : string.Empty,
                    SupplierName = item.s != null ? item.s.CompanyName : string.Empty
                }).ToList();

            return new PagedResult<InventoryTransaction>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public InventoryTransaction? GetById(int transactionId)
        {
            return _context.InventoryTransactions.AsNoTracking()
                .FirstOrDefault(t => t.TransactionId == transactionId);
        }

        public List<RevenueChartData> GetWeeklyRevenueChart()
        {
            var startDate = DateTime.Now.Date.AddDays(-6);

            var rawData = _context.InventoryTransactions
                .Where(t => t.TransactionType == TransactionAction.Sale && t.CreatedAt >= startDate)
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(t => t.TotalAmount),
                    Profit = g.Sum(t => t.Profit)
                }).ToList();

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

        public List<TopProductChartData> GetTopPerformingProducts()
        {
            return _context.InventoryTransactions
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
                .ToList();
        }
    }
}
