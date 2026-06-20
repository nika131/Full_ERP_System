using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IReportRepository
    {
        Task<CursorPagedResult<InventoryTransaction>> GetPagedTransactionsOptimized(
            int pageSize,
            DateTime? lastCreatedAt,
            int? lastTransactionId,
            int? productId,
            int? supplierId,
            int? searchTransactionId,
            int currentUserId,
            bool canViewAll,
            string typeFilter);
        Task<InventoryTransaction?> GetById(int transactionId);
        Task<List<RevenueChartData>> GetWeeklyRevenueChart();
        Task<List<TopProductChartData>> GetTopPerformingProducts();
    }
}
