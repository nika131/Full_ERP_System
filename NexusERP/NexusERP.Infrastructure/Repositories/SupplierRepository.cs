using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Domain.Models;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Infrastructure.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly ApplicationDbContext _context;

        public SupplierRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Supplier>> GetPaged(int companyId, int pageNumber, int pageSize, string? searchTerm)
        {
            var baseQuery = _context.Suppliers
                .Where(s => s.CompanyId == companyId)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                baseQuery = baseQuery.Where(s =>
                    s.CompanyName.Contains(searchTerm) ||
                    s.ContactName.Contains(searchTerm) ||
                    s.Email.Contains(searchTerm)
                );
            }

            var totalCount = await baseQuery.CountAsync();

            var items = await baseQuery
                .OrderBy(s => s.CompanyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Supplier>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<Supplier>> GetAllActive(int companyId)
        {
            return await _context.Suppliers
                .AsNoTracking()
                .Where(s => s.CompanyId == companyId)
                .Select(s => new Supplier
                {
                    SupplierId = s.SupplierId,
                    CompanyName = s.CompanyName,
                    CompanyId = s.CompanyId
                })
                .OrderBy(s => s.CompanyName)
                .ToListAsync();
        }

        public async Task Upsert(Supplier supplier, int userId, int companyId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    bool isNew = supplier.SupplierId == 0;

                    if (isNew)
                    {
                        supplier.CompanyId = companyId;
                        supplier.IsActive = true;

                        _context.Suppliers.Add(supplier);
                    }
                    else
                    {
                        var existing = await _context.Suppliers.FirstOrDefaultAsync(s =>
                            s.SupplierId == supplier.SupplierId &&
                            s.CompanyId == companyId
                        );

                        if (existing == null)
                            throw new AppException("Supplier not found.");

                        existing.CompanyName = supplier.CompanyName;
                        existing.ContactName = supplier.ContactName;
                        existing.Phone = supplier.Phone;
                        existing.Email = supplier.Email;
                    }

                    await _context.SaveChangesAsync();

                    var audit = new SystemAuditLog
                    {
                        CompanyId = companyId,
                        UserId = userId,
                        EntityType = "Supplier",
                        EntityId = supplier.SupplierId,
                        Action = isNew ? "Create" : "Update",
                        ChangesMade = isNew
                            ? $"Created Supplier '{supplier.CompanyName}'"
                            : $"Updated Supplier '{supplier.CompanyName}'"
                    };

                    _context.SystemAuditLogs.Add(audit);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task Delete(int id, int userId, int companyId)
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s =>
                s.SupplierId == id &&
                s.CompanyId == companyId
            );

            if (supplier == null)
                throw new AppException("Supplier not found.");

            supplier.IsActive = false;

            var audit = new SystemAuditLog
            {
                CompanyId = companyId,
                UserId = userId,
                EntityType = "Supplier",
                EntityId = supplier.SupplierId,
                Action = "Delete",
                ChangesMade = $"Deleted Supplier '{supplier.CompanyName}'"
            };

            _context.SystemAuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }
    }
}