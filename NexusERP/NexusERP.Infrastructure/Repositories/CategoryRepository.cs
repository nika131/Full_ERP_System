using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;
using NexusERP.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository (ApplicationDbContext context)
        {
            _context = context;
        }
        public PagedResult<Category> GetPagedCategories(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = _context.Categories.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.CategoryName.Contains(searchTerm));
            }

            var totalCount = query.Count();
            var items = query.OrderBy(c => c.CategoryName)
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            return new PagedResult<Category>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public IEnumerable<Category> GetAllActive()
        {
            return _context.Categories.AsNoTracking()
                .Select(c => new Category {  CategoryId = c.CategoryId, CategoryName = c.CategoryName })
                .OrderBy(c => c.CategoryName).ToList();
        }

        public void Upsert(Category category, int UserId)
        {
            if (category.CategoryId == 0)
            {
                _context.Categories.Add(category);

                var audit = new SystemAuditLog
                {
                    UserId = UserId,
                    EntityType = "Category",
                    EntityId = category.CategoryId,
                    Action = "Create",
                    ChangesMade = $"Created Category '{category.CategoryName}'"
                };

                _context.SystemAuditLogs.Add(audit);
            }
            else
            {
                _context.Categories.Update(category);

                var audit = new SystemAuditLog
                {
                    UserId = UserId,
                    EntityType = "Category",
                    EntityId = category.CategoryId,
                    Action = "Update",
                    ChangesMade = $"Updated Category '{category.CategoryName}'"
                };

                _context.SystemAuditLogs.Add(audit);
            }
            _context.SaveChanges();
        }

        public void Delete(int id, int UserId)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                category.IsActive = false;

                var audit = new SystemAuditLog
                {
                    UserId = UserId,
                    EntityType = "Category",
                    EntityId = category.CategoryId,
                    Action = "Delete",
                    ChangesMade = $"Delete Category '{category.CategoryName}'"
                };

                _context.SystemAuditLogs.Add(audit);
                _context.SaveChanges();
            }
        }
    }
}
