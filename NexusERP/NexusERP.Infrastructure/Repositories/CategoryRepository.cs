using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;
using NexusERP.Infrastructure.Database;
using NexusERP.Domain.Exceptions;
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
        public async Task<PagedResult<Category>> GetPagedCategories(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = _context.Categories
                .Where(c => c.IsActive)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.CategoryName.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var items = await query.OrderBy(c => c.CategoryName)
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

            return new PagedResult<Category>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<Category>> GetAllActive()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Select(c => new Category {  CategoryId = c.CategoryId, CategoryName = c.CategoryName })
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task Upsert(Category category, int UserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (category.CategoryId == 0)
                {
                    category.IsActive = true;
                    await _context.Categories.AddAsync(category);
                    await _context.SaveChangesAsync();

                    var audit = new SystemAuditLog
                    {
                        UserId = UserId,
                        EntityType = "Category",
                        EntityId = category.CategoryId,
                        Action = "Create",
                        ChangesMade = $"Created Category '{category.CategoryName}'"
                    };

                    await _context.SystemAuditLogs.AddAsync(audit);
                }
                else
                {
                    var existing = await _context.Categories.FindAsync(category.CategoryId);
                    if (existing == null || !existing.IsActive)
                        throw new AppException("Category not found or is inactive");

                    existing.CategoryName = category.CategoryName; 
                    
                    var audit = new SystemAuditLog
                    {
                        UserId = UserId,
                        EntityType = "Category",
                        EntityId = category.CategoryId,
                        Action = "Update",
                        ChangesMade = $"Updated Category '{category.CategoryName}'"
                    };

                    await _context.SystemAuditLogs.AddAsync(audit);
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
            var category = await _context.Categories.FindAsync(id);
            if (category == null || !category.IsActive)
                throw new AppException("Category not found");

            bool hasActiveProducts = await _context.Products.AnyAsync(p => p.CategoryId == id && p.IsActive);
            if (hasActiveProducts)
                throw new AppException("Cannot delete this category because it contains active products. Reassign the products first.");
            
            category.IsActive = false;

            var audit = new SystemAuditLog
            {
                UserId = UserId,
                EntityType = "Category",
                EntityId = category.CategoryId,
                Action = "Delete",
                ChangesMade = $"Delete Category '{category.CategoryName}'"
            };

            await _context.SystemAuditLogs.AddAsync(audit);
            await _context.SaveChangesAsync();
        }
    }
}
