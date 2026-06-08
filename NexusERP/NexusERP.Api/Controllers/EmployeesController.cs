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
    [Authorize(Roles = "Admin")]
    public class EmployeesController : Controller
    {
        private readonly IUserRepository _repository;

        public EmployeesController(IUserRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult GetEmployees([FromQuery] string? keyword, [FromBody] string roleFilter = "All")
        {
            IEnumerable<User> users;

            if (string.IsNullOrWhiteSpace(keyword))
                users = _repository.GetAllUsers();
            else
                users = _repository.SearchUsers(keyword);

            if (roleFilter != "All" && Enum.TryParse(roleFilter,out UserRole role))
            {
                users = users.Where(u => u.Role == role);
            }

            var responseData = users.Select(u => new EmployeeResponseDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Username = u.Username,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            }).ToList();

            return Ok(responseData);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEmployee(int id, [FromBody] EmployeeResponseDto dto)
        {
            if (!Enum.TryParse<UserRole>(dto.Role, true, out var parsedRole))
            {
                return BadRequest(new { message = "Invalid role specified." });
            }

            int currentUserId = User.GetCurrentUserId();

            if (id == currentUserId && parsedRole != UserRole.Admin)
            {
                return BadRequest(new { message = "Security Lock: You cannot remove your own Admin priviladges." });
            }

            var userToUpdate = new User
            {
                UserId = id,
                FullName = dto.FullName,
                Username = dto.Username,
                Role = parsedRole
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
                return BadRequest(new { message = "Security Lock: YOu cannot delete your own active account." });
            }

            _repository.DeleteUser(id);

            return Ok(new { message = "Employee access revoked successfully." });
        }

    }
}
