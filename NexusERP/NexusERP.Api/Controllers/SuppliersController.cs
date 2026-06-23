using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Application.DTOs;
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
        public async Task<IActionResult> GetPagedSuppliers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            if (pageSize > 100) pageSize = 100;

            var result = await _repository.GetPaged(page, pageSize, search);
            
            var responseItems = result.Items.Select(s => new SupplierResponseDto
            {
                SupplierId = s.SupplierId,
                CompanyName = s.CompanyName,
                ContactName = s.ContactName,
                Phone = s.Phone,
                Email = s.Email
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
            var suppliers = await _repository.GetAllActive();
            
            var lookupData = suppliers.Select(s => new SupplierLookupDto
            {
                SupplierId = s.SupplierId,
                CompanyName = s.CompanyName
            }).ToList();

            return Ok(lookupData);
        }

        [HttpPost("upsert")]
        public async Task<IActionResult> SaveSuppliers([FromBody] SupplierUpsertDto dto)
        {
            var supplier = new Supplier
            {
                SupplierId = dto.SupplierId,
                CompanyName = dto.CompanyName,
                ContactName = dto.ContactName,
                Phone = dto.Phone,
                Email = dto.Email
            };

            await _repository.Upsert(supplier, User.GetCurrentUserId());
            return Ok(new { message = "Supplier saved successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id) { 
            await _repository.Delete(id, User.GetCurrentUserId());
            return Ok(new { message = "Supplier deleted successfully." });
        }
    }
}
