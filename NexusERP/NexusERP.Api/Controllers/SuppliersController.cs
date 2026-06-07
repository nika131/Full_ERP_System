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
            try
            {
                if (pageSize > 100) pageSize = 100;
                var suppliers = _repository.GetPaged(page, pageSize, search);
                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Database Error: " + ex.Message });
            }
        }

        [HttpGet("lookup")]
        public IActionResult GetLookupList()
        {
            try
            {
                var suppliers = _repository.GetAllActive();
                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Database Error: " + ex.Message });
            }
        }

        [HttpPost("upsert")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult SaveSuppliers([FromBody] SupplierUpsertDto dto)
        {
            try
            {
                var supplier = new Supplier
                {
                    SupplierId = dto.SupplierId,
                    CompanyName = dto.CompanyName,
                    ContactName = dto.ContactName,
                    Phone = dto.Phone,
                    Email = dto.Email
                };

                _repository.UpsertSuppliers(supplier);
                return Ok(new { message = "Supplier saved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving supplier: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult DeleteSupplier(int id)
        {
            try
            {
                _repository.DeleteSupplier(id);
                return Ok(new { message = "Supplier deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting supplier: " + ex.Message });
            }
        }
    }
}
