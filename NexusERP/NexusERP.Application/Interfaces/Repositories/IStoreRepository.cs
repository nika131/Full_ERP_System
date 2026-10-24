using NetTopologySuite.Geometries;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IStoreRepository
    {
        Task<IEnumerable<Store>> GetAllStoresAsync(int companyId);
        Task<Store?> GetStoreByIdAsync(int companyId, int id);
        Task AddStoreAsync(Store store, int companyId);
        Task UpdateStoreAsync(Store store, int companyId);
        Task<IEnumerable<Store>> GetStoresWithinRadiusAsync(int companyId, Point location, double radiusInMeters);
        Task<PagedResult<Store>> GetPagedStoresAsync(int companyId, int pageNumber, int pageSize, string? searchTerm = null);
    }
}