using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.DTOs;
using NexusERP.Api.Extensions;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using System.Security.Claims;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Authorize(Policy = "RequireManageCategories")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _repository;

        public CategoriesController(ICategoryRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] int Page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            if (pageSize > 100) pageSize = 100;

            var result = await _repository.GetPagedCategories(Page, pageSize, search);
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
        public async Task<IActionResult> GetLookupList()
        {
            var categories = await _repository.GetAllActive();

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
            var category = new Category { 
                CategoryId = dto.CategoryId, 
                CategoryName = dto.Name,
            };

            await _repository.Upsert(category, User.GetCurrentUserId());
            return Ok(new { message = "Category saved." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _repository.Delete(id, User.GetCurrentUserId());
            return Ok(new { message = "Category deleted successfully." });
        }
    }
}

