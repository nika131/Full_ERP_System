using NetTopologySuite.Geometries;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IStoreRepository
    {
        Task<IEnumerable<Store>> GetAllStoresAsync();
        Task<Store?> GetStoreByIdAsync(int id);
        Task AddStoreAsync(Store store);
        Task UpdateStoreAsync(Store store);

        Task<IEnumerable<Store>> GetStoresWithinRadiusAsync(Point location, double radiusInMeters);

        Task<PagedResult<Store>> GetPagedStoresAsync(int pageNumber, int pageSize, string? searchTerm = null);
    }
}
