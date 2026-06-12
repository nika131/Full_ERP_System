using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Exceptions;
using NexusERP.Domain.Models;
using NexusERP.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Infrastructure.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly ApplicationDbContext _context;

        public SupplierRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<PagedResult<Supplier>> GetPaged(int pageNumber, int pageSize, string? searchTerm)
        {
            var baseQuery = _context.Suppliers
                .Where(s => s.IsActive)
                .AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                baseQuery = baseQuery.Where(s =>
                    s.CompanyName.Contains(searchTerm) ||
                    (s.ContactName != null && s.ContactName.Contains(searchTerm)) ||
                    (s.Email != null && s.Email.Contains(searchTerm)));
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

        public async Task<IEnumerable<Supplier>> GetAllActive()
        {
            return await _context.Suppliers
                .AsNoTracking()
                .Select(s => new Supplier
                {
                    SupplierId = s.SupplierId,
                    CompanyName = s.CompanyName
                })
                .OrderBy(s => s.CompanyName)
                .ToListAsync();
        }

        public async Task Upsert(Supplier supplier, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (supplier.SupplierId == 0)
                {
                    _context.Suppliers.Add(supplier);
                    await _context.SaveChangesAsync(); 

                    var audit = new SystemAuditLog
                    {
                        UserId = userId,
                        EntityType = "Supplier",
                        EntityId = supplier.SupplierId,
                        Action = "Create",
                        ChangesMade = $"Created Supplier '{supplier.CompanyName}'"
                    };
                    _context.SystemAuditLogs.Add(audit);
                }
                else
                {
                    var existing = await _context.Suppliers.FindAsync(supplier.SupplierId);
                    if (existing == null) throw new AppException("Supplier not found");

                    _context.Entry(existing).CurrentValues.SetValues(supplier);

                    var audit = new SystemAuditLog
                    {
                        UserId = userId,
                        EntityType = "Supplier",
                        EntityId = supplier.SupplierId,
                        Action = "Delete",
                        ChangesMade = $"Delete Supplier '{supplier.CompanyName}'"
                    };
                    _context.SystemAuditLogs.Add(audit);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task Delete(int id, int UserId)
        {
            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null || !supplier.IsActive) throw new AppException("Supplier not Found.");
     
            supplier.IsActive = false;

            var audit = new SystemAuditLog
            {
                UserId = UserId,
                EntityType = "Supplier",
                EntityId = supplier.SupplierId,
                Action = "Delete",
                ChangesMade = $"Deleted product '{supplier.CompanyName}'"
            };

            _context.SystemAuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }
    }
}
