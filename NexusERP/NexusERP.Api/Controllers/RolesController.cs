using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.Extensions;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Constants;
using NexusERP.Domain.Entities;
using System.Reflection;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RolesController : Controller
    {
        private readonly IRoleRepository _repository;

        public RolesController(IRoleRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.ManageRoles)]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            if (pageSize > 100) pageSize = 100;

            int companyId = User.GetCurrentCompanyId();

            var result = await _repository.GetPaged(companyId, page, pageSize, search);

            var responseItems = result.Items.Select(r => new RoleResponseDto
            {
                RoleId = r.RoleId,
                Name = r.Name,
                Permissions = r.Permissions,
                IsSystemRole = r.IsSystemRole
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
            var activeRoles = await _repository.GetAllActive(User.GetCurrentCompanyId());

            var roles = activeRoles.Select(r => new RoleLookupDto
            {
                RoleId = r.RoleId,
                Name = r.Name,
                IsSystemRole = r.IsSystemRole
            }).ToList();

            return Ok(roles);
        }

        [HttpGet("permissions")]
        [Authorize(Policy = Permissions.ManageRoles)]
        public IActionResult GetAvailablePermissions()
        {
            var permissions = Permissions.GetAllPermissions();

            return Ok(permissions);
        }

        [HttpPost("upsert")]
        [Authorize(Policy = Permissions.ManageRoles)]
        public async Task<IActionResult> SaveRole([FromBody] RoleUpsertDto dto)
        {
            var role = new Role
            {
                RoleId = dto.RoleId,
                Name = dto.Name,
                Permissions = dto.Permissions
            };

            await _repository.Upsert(role, User.GetCurrentUserId(), User.GetCurrentCompanyId());

            return Ok(new { message = "Role saved successfully." });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.ManageRoles)]
        public async Task<IActionResult> DeleteRole(int id)
        {
            await _repository.Delete(id, User.GetCurrentUserId(), User.GetCurrentCompanyId());

            return Ok(new { message = "Role deleted successfully." });
        }
    }
}