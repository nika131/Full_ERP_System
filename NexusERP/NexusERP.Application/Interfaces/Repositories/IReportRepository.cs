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
            DateTime? endDate = null);

        Task<InventoryTransaction?> GetById(int transactionId);

        Task<List<RevenueChartData>> GetWeeklyRevenueChart(DateTime? startDate, DateTime? endDate, int? storeId, int? categoryId, int? supplierId);

        Task<List<TopProductChartData>> GetTopPerformingProducts(DateTime? startDate, DateTime? endDate, int? storeId, int? categoryId, int? supplierId);
    }
}
