using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Application.DTOs;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<(PagedResult<Product> Result, decimal totalValue)> GetPaged(int pageNumber, int pageSize, string? searchTerm, string? categoryName, string? supplierName, bool lowStockOnly = false);
        Task Upsert(Product product, int userId);
        Task SaveTransaction(InventoryTransaction transaction, Product product, SystemAuditLog audit);
        Task<Product> GetByIdAsync(int id);
        Task Delete(int id, int userId);
        Task<DashboardResponse> GetDashboardAggregates(DashboardFilterRequest request);
        Task<List<Product>> GetProductsLowOnStock();
    }
}
