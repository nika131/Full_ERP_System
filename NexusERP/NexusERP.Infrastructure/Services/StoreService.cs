using NetTopologySuite;
using NetTopologySuite.Geometries;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<IEnumerable<StoreDto>> GetAllStoresAsync()
        {
            var stores = await _storeRepository.GetAllStoresAsync();
            return stores.Select(MapToDto);
        }

        public async Task<StoreDto?> GetStoreByIdAsync(int id)
        {
            var store = await _storeRepository.GetStoreByIdAsync(id);
            return store == null ? null : MapToDto(store);
        }

        public async Task<StoreDto> CreateStoreAsync(CreateStoreDto dto)
        {
            var store = new Store
            {
                Name = dto.Name,
                Address = dto.Address,
                Location = _geometryFactory.CreatePoint(new Coordinate(dto.Longitude, dto.Latitude)),
                IsActive = true
            };

            await _storeRepository.AddStoreAsync(store);
            return MapToDto(store);
        }

        public async Task UpdateStoreAsync(int id, UpdateStoreDto dto)
        {
            var store = await _storeRepository.GetStoreByIdAsync(id)
                ?? throw new AppException($"Store with ID {id} not found.");

            store.Name = dto.Name;
            store.Address = dto.Address;
            store.Location = _geometryFactory.CreatePoint(new Coordinate(dto.Longitude, dto.Latitude));
            store.IsActive = dto.IsActive;

            await _storeRepository.UpdateStoreAsync(store);
        }

        public async Task<IEnumerable<StoreDto>> GetStoresNearbyAsync(double latitude, double longitude, double radiusInMeters)
        {
            var centerPoint = _geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

            var stores = await _storeRepository.GetStoresWithinRadiusAsync(centerPoint, radiusInMeters);

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
                IsActive = store.IsActive
            };
        }
    }
}
