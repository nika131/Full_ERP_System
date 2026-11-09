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
    
    public class SuppliersController : Controller
    {
        private readonly ISupplierRepository _repository;
        private readonly ApplicationDbContext _db;

        public SuppliersController(ISupplierRepository repository, ApplicationDbContext db)
        {
            _repository = repository;
            _db = db;
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

        [HttpGet("{id:int}/deletion-impact")]
        [Authorize(Policy = Permissions.DeleteSuppliers)]
        public async Task<IActionResult> GetDeletionImpact(int id)
        {
            var companyId = User.GetCurrentCompanyId();
            if (!await _db.Suppliers.AnyAsync(s => s.CompanyId == companyId && s.SupplierId == id))
                return NotFound();
            var activeProducts = await _db.Products.CountAsync(p => p.CompanyId == companyId && p.SupplierId == id);
            return Ok(new { activeProducts });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = Permissions.DeleteSuppliers)]
        public async Task<IActionResult> DeleteSupplier(int id, [FromQuery] bool confirm = false)
        {
            if (!confirm) return BadRequest(new { message = "Deletion confirmation is required." });
            await _repository.Delete(id, User.GetCurrentUserId(), User.GetCurrentCompanyId());

            return Ok(new { message = "Supplier deleted successfully." });
        }
    }
}