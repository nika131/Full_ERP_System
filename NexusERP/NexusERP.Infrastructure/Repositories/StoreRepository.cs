using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Domain.Models;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Infrastructure.Repositories
{
    public class StoreRepository : IStoreRepository
    {
        private readonly ApplicationDbContext _context;

        public StoreRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Store>> GetPagedStoresAsync(int companyId, int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.Stores
                .Where(s => s.CompanyId == companyId)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(s => s.Name.Contains(searchTerm) || s.Address.Contains(searchTerm));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.StoreId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Store>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<Store>> GetAllStoresAsync(int companyId)
        {
            return await _context.Stores
                .Where(s => s.CompanyId == companyId)
                .AsNoTracking()
                .Select(s => new Store
                {
                    StoreId = s.StoreId,
                    CompanyId = s.CompanyId,
                    Name = s.Name,
                    Address = s.Address,
                    Location = s.Location,
                    MaxCartDiscountPercentage = s.MaxCartDiscountPercentage,
                    IsActive = s.IsActive
                })
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Store?> GetStoreByIdAsync(int companyId, int id)
        {
            return await _context.Stores.FirstOrDefaultAsync(s =>
                s.StoreId == id &&
                s.CompanyId == companyId
            );
        }

        public async Task AddStoreAsync(Store store, int companyId)
        {

            store.Name = store.Name?.Trim() ?? string.Empty;

            if (store.Name.Length is < 2 or > 100)
                throw new AppException("Store name must contain 2–100 characters.");

            bool nameExists = await _context.Stores.AnyAsync(s =>
                s.CompanyId == companyId &&
                s.IsActive &&
                s.Name == store.Name
            );

            if (nameExists)
                throw new AppException("An active store with this name already exists.");

            store.CompanyId = companyId;

            await _context.Stores.AddAsync(store);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStoreAsync(Store store, int companyId)
        {
            store.Name = store.Name?.Trim() ?? string.Empty;

            if (store.Name.Length is < 2 or > 100)
                throw new AppException("Store name must contain 2–100 characters.");

            if (store.IsActive)
            {
                bool nameExists = await _context.Stores.AnyAsync(s =>
                    s.CompanyId == companyId &&
                    s.IsActive &&
                    s.Name == store.Name &&
                    s.StoreId != store.StoreId
                );

                if (nameExists)
                    throw new AppException("An active store with this name already exists.");
            }

            var existing = await _context.Stores.FirstOrDefaultAsync(s =>
                s.StoreId == store.StoreId &&
                s.CompanyId == companyId
            );

            if (existing == null)
                throw new AppException("Store not found.");

            existing.Name = store.Name;
            existing.Address = store.Address;
            existing.Location = store.Location;
            existing.IsActive = store.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Store>> GetStoresWithinRadiusAsync(int companyId, Point location, double radiusInMeters)
        {
            return await _context.Stores
                .Where(s =>
                    s.CompanyId == companyId &&
                    s.Location.Distance(location) <= radiusInMeters
                )
                .AsNoTracking()
                .ToListAsync();
        }
    }
}