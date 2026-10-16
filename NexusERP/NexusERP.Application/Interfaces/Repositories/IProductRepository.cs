using NexusERP.Application.DTOs;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<(PagedResult<Product> Result, decimal totalValue)> GetPaged(int companyId, int pageNumber, int pageSize, string? searchTerm, string? categoryName, string? supplierName, bool lowStockOnly = false, List<int>? excludeIds = null);
        Task Upsert(Product product, int userId, int companyId);
        Task SaveTransaction(InventoryTransaction transaction, Product product, SystemAuditLog audit, int companyId);
        Task<Product> GetByIdAsync(int companyId, int id);
        Task Delete(int id, int userId, int companyId);
        Task<DashboardResponse> GetDashboardAggregates(int companyId, DashboardFilterRequest request);
        Task<List<Product>> GetProductsLowOnStock(int companyId);
        Task<bool> IsNegativeInventoryAllowedAsync(int companyId);
    }
}