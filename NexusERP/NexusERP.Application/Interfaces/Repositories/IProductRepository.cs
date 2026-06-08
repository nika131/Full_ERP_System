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
        PagedResult<Product> GetPaged(int pageNumber, int pageSize, string? searchTerm);
        void Upsert(Product product, int userId);
        void LogInventoryTransaction(InventoryTransaction transaction, int userId, string transactionType);
        void Delete(int id, int userId);
        DashboardResponse GetDashboardAggregates();
    }
}
