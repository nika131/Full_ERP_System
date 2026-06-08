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
        public PagedResult<Supplier> GetPaged(int pageNumber, int pageSize, string? searchTerm)
        {
            var baseQuery = _context.Suppliers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                baseQuery = baseQuery.Where(s =>
                    s.CompanyName.Contains(searchTerm) ||
                    (s.ContactName != null && s.ContactName.Contains(searchTerm)) ||
                    (s.Email != null && s.Email.Contains(searchTerm)));
            }

            var totalCount = baseQuery.Count();

            var items = baseQuery
                .OrderBy(s => s.CompanyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Supplier>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public IEnumerable<Supplier> GetAllActive()
        {
            return _context.Suppliers
                .AsNoTracking()
                .Select(s => new Supplier
                {
                    SupplierId = s.SupplierId,
                    CompanyName = s.CompanyName
                })
                .OrderBy(s => s.CompanyName)
                .ToList();
        }

        public void Upsert(Supplier supplier, int userId)
        {
            if (supplier.SupplierId == 0)
            {
                _context.Suppliers.Add(supplier);

                var audit = new SystemAuditLog
                {
                    UserId = userId,
                    EntityType = "Supplier",
                    EntityId = supplier.SupplierId,
                    Action = "Create",
                    ChangesMade = $"Created product '{supplier.CompanyName}'"
                };

                _context.SystemAuditLogs.Add(audit);
            }
            else
            {
                var existing = _context.Suppliers.AsNoTracking()
                               .FirstOrDefault(s => s.SupplierId == supplier.SupplierId);

                if (existing == null) throw new AppException("Supplier not Found");

                _context.Suppliers.Update(supplier);

                var audit = new SystemAuditLog
                {
                    UserId = userId,
                    EntityType = "Supplier",
                    EntityId = supplier.SupplierId,
                    Action = "Update",
                    ChangesMade = $"Updated product '{supplier.CompanyName}'"
                };

                _context.SystemAuditLogs.Add(audit);
            }

            _context.SaveChanges();
        }

        public void Delete(int id, int UserId)
        {
            var supplier = _context.Suppliers.Find(id);

            if (supplier != null)
            {
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
                _context.SaveChanges();
            }
        }
    }
}
