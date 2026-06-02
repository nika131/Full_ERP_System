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
        public IActionResult GetAllSuppliers()
        {
            try
            {
                var suppliers = _repository.GetAllSuppliers();
                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error loading supplier: " + ex.Message });
            }
        }

        [HttpGet("search")]
        public IActionResult SearchSuppliers([FromQuery] string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return BadRequest(new { meesage = "Keyword is required for searching." });
            }

            try
            {
                var suppliers = _repository.SearchSuppliers(keyword);
                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { meesage = "Error searching suppliers: " + ex.Message });
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
                return StatusCode(500, new { meesage = "Error saving supplier: " + ex.Message });
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
