using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using System.Security.Claims;
using NexusERP.Api.DTOs;
using DocumentFormat.OpenXml.Bibliography;
using NexusERP.Api.Extensions;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireManageUsers")]
    public class EmployeesController : Controller
    {
        private readonly IUserRepository _repository;

        public EmployeesController(IUserRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string roleFilter = "All"
        )
        {
            if (pageSize > 100) pageSize = 100;

            var result = await _repository.GetPagedAsync(page, pageSize, searchTerm, roleFilter);

            var responseData = result.Items.Select(u => new EmployeeResponseDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Username = u.Username,
                RoleId = u.RoleId,
                RoleName = u.Role?.Name ?? "Unassigned",
                CreatedAt = u.CreatedAt
            }).ToList();

            return Ok(new
            {
                items = responseData,
                totalCount = result.TotalCount,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeUpdateDto dto)
        {
            int currentUserId = User.GetCurrentUserId();

            if (id == currentUserId)
            {
                return BadRequest(new { message = "Security Lock: You cannot remove your own Admin priviladges." });
            }

            var userToUpdate = new User
            {
                UserId = id,
                FullName = dto.FullName,
                Username = dto.Username,
                RoleId = dto.RoleId,
            };

            await _repository.UpdateUser(userToUpdate, currentUserId);
            return Ok(new { message = "Employee updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            int currentUserId = User.GetCurrentUserId();

            if (id == currentUserId)
            {
                return BadRequest(new { message = "Security Lock: You cannot delete your own active account." });
            }

            await _repository.DeleteUser(id, currentUserId);

            return Ok(new { message = "Employee access revoked successfully." });
        }

    }
}
