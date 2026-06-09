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
        public IActionResult GetEmployees([FromQuery] string? keyword, [FromQuery] string roleFilter = "All")
        {
            IEnumerable<User> users;

            if (string.IsNullOrWhiteSpace(keyword))
                users = _repository.GetAllUsers();
            else
                users = _repository.SearchUsers(keyword);

            if (roleFilter != "All")
            {
                users = users.Where(u => u.Role != null && u.Role.Name.Equals(roleFilter, StringComparison.OrdinalIgnoreCase));
            }

            var responseData = users.Select(u => new EmployeeResponseDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Username = u.Username,
                RoleId = u.RoleId,
                RoleName = u.Role?.Name ?? "Unassigned",
                CreatedAt = u.CreatedAt
            }).ToList();

            return Ok(responseData);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEmployee(int id, [FromBody] EmployeeResponseDto dto)
        {
            if (id == User.GetCurrentUserId()) return BadRequest(new { message = "Security Lock: You cannot remove your own Admin priviladges." });

            var userToUpdate = new User
            {
                UserId = id,
                FullName = dto.FullName,
                Username = dto.Username,
                RoleId = dto.RoleId,
            };

            _repository.UpdateUser(userToUpdate);

            return Ok(new { message = "Employee updated successfully." });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteEmployee(int id)
        {
            int currentUserId = User.GetCurrentUserId();

            if (id == currentUserId)
            {
                return BadRequest(new { message = "Security Lock: You cannot delete your own active account." });
            }

            _repository.DeleteUser(id);

            return Ok(new { message = "Employee access revoked successfully." });
        }

    }
}
