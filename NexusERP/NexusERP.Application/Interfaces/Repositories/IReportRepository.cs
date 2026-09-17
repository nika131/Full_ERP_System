using NexusERP.Application.DTOs;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        Task<List<RevenueChartData>> GetWeeklyRevenueChart(DashboardFilterRequest request);

        Task<List<TopProductChartData>> GetTopPerformingProducts(DashboardFilterRequest request);
        Task<PagedResult<ShiftAuditDto>> GetPagedShiftsAsync(int pageNumber, int pageSize, int? storeId);
        Task<PagedResult<ReceiptAuditDto>> GetPagedReceiptsAsync(int pageNumber, int pageSize, string? receiptNumber);
    }
}
