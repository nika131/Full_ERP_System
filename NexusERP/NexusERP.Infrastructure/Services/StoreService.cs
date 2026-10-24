using NetTopologySuite;
using NetTopologySuite.Geometries;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Domain.Models;

namespace NexusERP.Infrastructure.Services
{
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly GeometryFactory _geometryFactory;

        public StoreService(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
            _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        }

        public async Task<PagedResult<StoreDto>> GetPagedStoresAsync(int companyId, int pageNumber, int pageSize, string? searchTerm = null)
        {
            var pagedResult = await _storeRepository.GetPagedStoresAsync(companyId, pageNumber, pageSize, searchTerm);

            return new PagedResult<StoreDto>
            {
                Items = pagedResult.Items.Select(MapToDto).ToList(),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<IEnumerable<StoreDto>> GetAllStoresAsync(int companyId)
        {
            var stores = await _storeRepository.GetAllStoresAsync(companyId);
            return stores.Select(MapToDto);
        }

        public async Task<StoreDto?> GetStoreByIdAsync(int companyId, int id)
        {
            var store = await _storeRepository.GetStoreByIdAsync(companyId, id);
            return store == null ? null : MapToDto(store);
        }

        public async Task<StoreDto> CreateStoreAsync(int companyId, CreateStoreDto dto)
        {
            var store = new Store
            {
                CompanyId = companyId,
                Name = dto.Name,
                Address = dto.Address,
                Location = _geometryFactory.CreatePoint(new Coordinate(dto.Longitude, dto.Latitude)),
                IsActive = true
            };

            await _storeRepository.AddStoreAsync(store, companyId);

            return MapToDto(store);
        }

        public async Task UpdateStoreAsync(int companyId, int id, UpdateStoreDto dto)
        {
            var store = await _storeRepository.GetStoreByIdAsync(companyId, id)
                ?? throw new AppException($"Store with ID {id} not found.");

            store.Name = dto.Name;
            store.Address = dto.Address;
            store.Location = _geometryFactory.CreatePoint(new Coordinate(dto.Longitude, dto.Latitude));
            store.IsActive = dto.IsActive;

            await _storeRepository.UpdateStoreAsync(store, companyId);
        }

        public async Task<IEnumerable<StoreDto>> GetStoresNearbyAsync(int companyId, double latitude, double longitude, double radiusInMeters)
        {
            var centerPoint = _geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
            var stores = await _storeRepository.GetStoresWithinRadiusAsync(companyId, centerPoint, radiusInMeters);

            return stores.Select(MapToDto);
        }

        private static StoreDto MapToDto(Store store)
        {
            return new StoreDto
            {
                StoreId = store.StoreId,
                Name = store.Name,
                Address = store.Address,
                Latitude = store.Location.Y,
                Longitude = store.Location.X,
                MaxCartDiscountPercentage = store.MaxCartDiscountPercentage,
                IsActive = store.IsActive
            };
        }
    }
}