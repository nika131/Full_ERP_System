using NexusERP.Application.DTOs;
using NexusERP.Domain.Models;

namespace NexusERP.Application.Interfaces.Services
{
    public interface IStoreService
    {
        Task<IEnumerable<StoreDto>> GetAllStoresAsync(int companyId);
        Task<StoreDto?> GetStoreByIdAsync(int companyId, int id);
        Task<StoreDto> CreateStoreAsync(int companyId, CreateStoreDto dto);
        Task UpdateStoreAsync(int companyId, int id, UpdateStoreDto dto);
        Task<IEnumerable<StoreDto>> GetStoresNearbyAsync(int companyId, double latitude, double longitude, double radiusInMeters);
        Task<PagedResult<StoreDto>> GetPagedStoresAsync(int companyId, int pageNumber, int pageSize, string? searchTerm = null);
    }
}