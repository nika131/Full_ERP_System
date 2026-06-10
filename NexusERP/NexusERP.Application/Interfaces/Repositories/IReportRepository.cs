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
        Task<PagedResult<InventoryTransaction>> GetPagedTransactions(int pageNumber, int pageSize, string? searchTerm, int currentUserId, bool canViewAll, string typeFilter);
        Task<InventoryTransaction?> GetById(int transactionId);
        Task<List<RevenueChartData>> GetWeeklyRevenueChart();
        Task<List<TopProductChartData>> GetTopPerformingProducts();
    }
}
