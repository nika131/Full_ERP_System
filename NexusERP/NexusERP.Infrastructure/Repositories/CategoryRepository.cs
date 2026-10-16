using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Domain.Models;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Category>> GetPagedCategories(int companyId, int pageNumber, int pageSize, string? searchTerm)
        {
            var query = _context.Categories
                .Where(c => c.CompanyId == companyId)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(c => c.CategoryName.Contains(searchTerm));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.CategoryName)
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

        public async Task<IEnumerable<Category>> GetAllActive(int companyId)
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.CompanyId == companyId)
                .Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CompanyId = c.CompanyId
                })
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task Upsert(Category category, int userId, int companyId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    bool isNew = category.CategoryId == 0;

                    if (isNew)
                    {
                        category.CompanyId = companyId;
                        category.IsActive = true;

                        await _context.Categories.AddAsync(category);
                    }
                    else
                    {
                        var existing = await _context.Categories.FirstOrDefaultAsync(c =>
                            c.CategoryId == category.CategoryId &&
                            c.CompanyId == companyId
                        );

                        if (existing == null)
                            throw new AppException("Category not found or is inactive.");

                        existing.CategoryName = category.CategoryName;
                    }

                    await _context.SaveChangesAsync();

                    var audit = new SystemAuditLog
                    {
                        CompanyId = companyId,
                        UserId = userId,
                        EntityType = "Category",
                        EntityId = category.CategoryId,
                        Action = isNew ? "Create" : "Update",
                        ChangesMade = isNew
                            ? $"Created Category '{category.CategoryName}'"
                            : $"Updated Category '{category.CategoryName}'"
                    };

                    await _context.SystemAuditLogs.AddAsync(audit);
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
            var category = await _context.Categories.FirstOrDefaultAsync(c =>
                c.CategoryId == id &&
                c.CompanyId == companyId
            );

            if (category == null)
                throw new AppException("Category not found.");

            bool hasActiveProducts = await _context.Products.AnyAsync(p =>
                p.CompanyId == companyId &&
                p.CategoryId == id &&
                p.IsActive
            );

            if (hasActiveProducts)
                throw new AppException("Cannot delete this category because it contains active products. Reassign the products first.");

            category.IsActive = false;

            var audit = new SystemAuditLog
            {
                CompanyId = companyId,
                UserId = userId,
                EntityType = "Category",
                EntityId = category.CategoryId,
                Action = "Delete",
                ChangesMade = $"Deleted Category '{category.CategoryName}'"
            };

            await _context.SystemAuditLogs.AddAsync(audit);
            await _context.SaveChangesAsync();
        }
    }
}