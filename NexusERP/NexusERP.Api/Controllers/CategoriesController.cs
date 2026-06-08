using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.DTOs;
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

        public CategoriesController(ICategoryRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult GetPaged([FromQuery] int Page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            if (pageSize > 100) pageSize = 100;
            return Ok(_repository.GetPagedCategories(Page, pageSize, search));
        }

        [HttpGet("lookup")]
        public IActionResult GetLookupList()
        {
            return Ok(_repository.GetAllActive());
        }

        [HttpPost("upsert")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult SaveCategory([FromBody] CategoryUpsertDto dto)
        {
            var category = new Category { CategoryId = dto.CategoryId, CategoryName = dto.CategoryName };
            _repository.Upsert(category);
            return Ok(new { message = "Category saved." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult DeleteCategory(int id)
        {
            _repository.Delete(id);
            return Ok(new { message = "Catgeory delete." });
        }
    }
}
