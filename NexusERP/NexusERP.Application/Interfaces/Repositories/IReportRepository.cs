using NexusERP.Application.DTOs;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IReportRepository
    {
        Task<CursorPagedResult<InventoryTransaction>> GetPagedTransactionsOptimized(int companyId, int pageSize, int currentUserId, bool canViewAll, string typeFilter, string? searchTerm, DateTime? lastCreatedAt, int? lastTransactionId, int? productId, int? supplierId, int? storeId = null, int? categoryId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<InventoryTransaction?> GetById(int companyId, int transactionId);
        Task<List<RevenueChartData>> GetWeeklyRevenueChart(int companyId, DashboardFilterRequest request);
        Task<List<TopProductChartData>> GetTopPerformingProducts(int companyId, DashboardFilterRequest request);
        Task<PagedResult<ShiftAuditDto>> GetPagedShiftsAsync(int companyId, int pageNumber, int pageSize, int? storeId);
        Task<PagedResult<ReceiptAuditDto>> GetPagedReceiptsAsync(int companyId, int pageNumber, int pageSize, string? receiptNumber);
    }
}