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
            return Ok(result);
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetLookupList()
        {
            var categories = await _repository.GetAllActive();
            return Ok(categories);
        }

        [HttpPost("upsert")]
        public async Task<IActionResult> SaveCategory([FromBody] CategoryUpsertDto dto)
        {
            var category = new Category { CategoryId = dto.CategoryId, CategoryName = dto.CategoryName };

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

