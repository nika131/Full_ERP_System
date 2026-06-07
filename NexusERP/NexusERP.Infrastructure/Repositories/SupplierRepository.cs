using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
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

        public void UpsertSuppliers(Supplier supplier)
        {
            if (supplier.SupplierId == 0)
            {
                _context.Suppliers.Add(supplier);
            }
            else
            {
                supplier.UpdatedAt = DateTime.Now;
                _context.Suppliers.Update(supplier);
            }

            _context.SaveChanges();
        }

        public void DeleteSupplier(int id)
        {
            var supplier = _context.Suppliers.Find(id);

            if (supplier != null)
            {
                supplier.IsActive = false;
                supplier.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }
        }
    }
}
