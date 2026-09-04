using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Infrastructure.Repositories
{
    public class StoreRepository : IStoreRepository
    {
        private readonly ApplicationDbContext _context;

        public StoreRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Store>> GetAllStoresAsync()
        {
            return await _context.Stores
                .Where(s => s.IsActive)
                .Select(s => new Store
                {
                    StoreId = s.StoreId,
                    Name = s.Name,
                    Address = s.Address,
                    Location = s.Location,
                })
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Store?> GetStoreByIdAsync(int id)
        {
            return await _context.Stores.FirstOrDefaultAsync(s => s.StoreId == id);
        }

        public async Task AddStoreAsync(Store store)
        {
            await _context.Stores.AddAsync(store);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStoreAsync(Store store)
        {
            _context.Stores.Update(store);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Store>> GetStoresWithinRadiusAsync(Point location, double radiusInMeteres)
        {
            return await _context.Stores
                .Where(s => s.Location.Distance(location) <= radiusInMeteres)
                .ToListAsync();
        }
    }
}
