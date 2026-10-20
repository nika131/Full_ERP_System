using NexusERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.Extensions;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class SuppliersController : Controller
    {
        private readonly ISupplierRepository _repository;

        public SuppliersController(ISupplierRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.ViewSuppliers)]
        public async Task<IActionResult> GetPagedSuppliers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            if (pageSize > 100) pageSize = 100;

            var result = await _repository.GetPaged(User.GetCurrentCompanyId(), page, pageSize, search);

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
        [Authorize(Policy = "RequireSupplierLookup")]
        public async Task<IActionResult> GetLookupList()
        {
            var suppliers = await _repository.GetAllActive(User.GetCurrentCompanyId());

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
            string requiredPermission = dto.SupplierId == 0 ? Permissions.CreateSuppliers : Permissions.EditSuppliers;
            if (!User.HasPermission(requiredPermission))
                return StatusCode(403, new { message = "You do not have permission to create or edit suppliers." });

            var supplier = new Supplier
            {
                SupplierId = dto.SupplierId,
                CompanyName = dto.CompanyName,
                ContactName = dto.ContactName,
                Phone = dto.Phone,
                Email = dto.Email
            };

            await _repository.Upsert(supplier, User.GetCurrentUserId(), User.GetCurrentCompanyId());

            return Ok(new { message = "Supplier saved successfully." });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.DeleteSuppliers)]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            await _repository.Delete(id, User.GetCurrentUserId(), User.GetCurrentCompanyId());

            return Ok(new { message = "Supplier deleted successfully." });
        }
    }
}