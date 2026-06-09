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
    [Authorize(Policy = "RequireManageSuppliers")]
    public class SuppliersController : Controller
    {
        private readonly ISupplierRepository _repository;

        public SuppliersController(ISupplierRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult GetPagedSuppliers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            if (pageSize > 100) pageSize = 100;
            var suppliers = _repository.GetPaged(page, pageSize, search);
            return Ok(suppliers);
        }

        [HttpGet("lookup")]
        public IActionResult GetLookupList()
        {
            var suppliers = _repository.GetAllActive();
            return Ok(suppliers);
        }

        [HttpPost("upsert")]
        public IActionResult SaveSuppliers([FromBody] SupplierUpsertDto dto)
        {
            var supplier = new Supplier
            {
                SupplierId = dto.SupplierId,
                CompanyName = dto.CompanyName,
                ContactName = dto.ContactName,
                Phone = dto.Phone,
                Email = dto.Email
            };

            _repository.Upsert(supplier, User.GetCurrentUserId());
            return Ok(new { message = "Supplier saved successfully." });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSupplier(int id)
        {
            _repository.Delete(id, User.GetCurrentUserId());
            return Ok(new { message = "Supplier deleted successfully." });
        }
    }
}
