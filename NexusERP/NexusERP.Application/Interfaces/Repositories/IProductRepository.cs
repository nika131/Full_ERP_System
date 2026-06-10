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
    public interface IProductRepository
    {
        Task<PagedResult<Product>> GetPaged(int pageNumber, int pageSize, string? searchTerm);
        Task Upsert(Product product, int userId);
        Task LogInventoryTransaction(InventoryTransaction transaction, int userId, string transactionType);
        Task Delete(int id, int userId);
        Task<DashboardResponse> GetDashboardAggregates();
    }
}
