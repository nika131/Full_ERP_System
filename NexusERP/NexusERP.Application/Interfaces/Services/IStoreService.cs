using NexusERP.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Services
{
    public interface IStoreService
    {
        Task<IEnumerable<StoreDto>> GetAllStoresAsync();
        Task<StoreDto?> GetStoreByIdAsync(int id);
        Task<StoreDto> CreateStoreAsync(CreateStoreDto dto);
        Task UpdateStoreAsync(int id, UpdateStoreDto dto);

        Task<IEnumerable<StoreDto>> GetStoresNearbyAsync(double latitude, double longitude, double radiusInMeters);
    }
}
