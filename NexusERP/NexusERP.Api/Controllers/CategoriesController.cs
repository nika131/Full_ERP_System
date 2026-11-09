using NexusERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusERP.Infrastructure.Database;
using NexusERP.Api.Extensions;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _repository;
        private readonly ApplicationDbContext _db;

        public CategoriesController(ICategoryRepository repository, ApplicationDbContext db)
        {
            _repository = repository;
            _db = db;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.ViewCategories)]
        public async Task<IActionResult> GetPaged([FromQuery] int Page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            if (pageSize > 100) pageSize = 100;

            var result = await _repository.GetPagedCategories(User.GetCurrentCompanyId(), Page, pageSize, search);

            var responseItems = result.Items.Select(c => new CategoryResponseDto
            {
                CategoryId = c.CategoryId,
                Name = c.CategoryName,
                CreatedAt = c.CreatedAt
            }).ToList();

            return Ok(new
            {
                items = responseItems,
                totalCount = result.TotalCount,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize
            });
        }

        [HttpGet("lookup")]
        [Authorize(Policy = "RequireCategoryLookup")]
        public async Task<IActionResult> GetLookupList()
        {
            var categories = await _repository.GetAllActive(User.GetCurrentCompanyId());

            var lookupData = categories.Select(c => new CategoryLookupDto
            {
                CategoryId = c.CategoryId,
                Name = c.CategoryName
            }).ToList();

            return Ok(lookupData);
        }

        [HttpPost("upsert")]
        public async Task<IActionResult> SaveCategory([FromBody] CategoryUpsertDto dto)
        {
            string requiredPermission = dto.CategoryId == 0 ? Permissions.CreateCategories : Permissions.EditCategories;
            if (!User.HasPermission(requiredPermission))
                return StatusCode(403, new { message = "You do not have permission to create or edit categories." });

            var category = new Category
            {
                CategoryId = dto.CategoryId,
                CategoryName = dto.Name
            };

            await _repository.Upsert(category, User.GetCurrentUserId(), User.GetCurrentCompanyId());

            return Ok(new { message = "Category saved." });
        }

        [HttpGet("{id:int}/deletion-impact")]
        [Authorize(Policy = Permissions.DeleteCategories)]
        public async Task<IActionResult> GetDeletionImpact(int id)
        {
            var companyId = User.GetCurrentCompanyId();
            if (!await _db.Categories.AnyAsync(c => c.CompanyId == companyId && c.CategoryId == id))
                return NotFound();
            var activeProducts = await _db.Products.CountAsync(p => p.CompanyId == companyId && p.CategoryId == id);
            return Ok(new { activeProducts });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = Permissions.DeleteCategories)]
        public async Task<IActionResult> DeleteCategory(int id, [FromQuery] bool confirm = false)
        {
            if (!confirm) return BadRequest(new { message = "Deletion confirmation is required." });
            await _repository.Delete(id, User.GetCurrentUserId(), User.GetCurrentCompanyId());

            return Ok(new { message = "Category deleted successfully." });
        }
    }
}