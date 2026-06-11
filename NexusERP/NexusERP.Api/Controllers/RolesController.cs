using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.DTOs;
using NexusERP.Api.Extensions;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Constants;
using NexusERP.Domain.Entities;
using System.Reflection;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireManageUsers")]
    public class RolesController : Controller
    {
        private readonly IRoleRepository _repository;

        public RolesController(IRoleRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            if (pageSize > 100) pageSize = 100;

            var result = await _repository.GetPaged(page, pageSize, search);

            var responseItems = result.Items.Select(r => new RoleResponseDto
            {
                RoleId = r.RoleId,
                Name = r.Name,
                Permissions = r.Permissions
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
            var activeRoles = await _repository.GetAllActive();

            var roles = activeRoles.Select(r => new RoleLookupDto
            {
                RoleId = r.RoleId,
                Name = r.Name
            }).ToList();

            return Ok(roles);
        }

        [HttpGet("permissions")]
        public IActionResult GetAvailablePermissions()
        {
            var permissions = typeof(Permissions)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .Select(fi => fi.GetRawConstantValue()?.ToString())
                .ToList();

            return Ok(permissions);
        }

        [HttpPost("upsert")]
        public async Task<IActionResult> SaveRole([FromBody] RoleUpsertDto dto)
        {
            var role = new Role
            {
                RoleId = dto.RoleId,
                Name = dto.Name,
                Permissions = dto.Permissions
            };

            await _repository.Upsert(role, User.GetCurrentUserId());
            return Ok(new { message = "Role saved successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            await _repository.Delete(id, User.GetCurrentUserId());
            return Ok(new { message = "Role deleted successfully." });
        }
    }
}
