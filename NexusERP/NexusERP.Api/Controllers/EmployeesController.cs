using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.DTOs;
using NexusERP.Api.Extensions;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using System.Security.Claims;
using static NexusERP.Api.DTOs.SalaryDtos;

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

        [HttpGet("{id}/salary")]
        public async Task<IActionResult> GetSalaryHistory(int id)
        {
            var history = await _repository.GetSalaryHistoryAsync(id);

            var safeData = history.Select(s => new SalaryRecordResponseDto
            {
                SalaryRecordId = s.SalaryRecordId,
                Amount = s.Amount,
                EffectiveDate = s.EffectiveDate,
                Notes = s.Notes,
                CreatedAt = s.CreatedAt
            }).ToList();

            return Ok(safeData);
        }

        [HttpPost("{id}/salary")]
        public async Task<IActionResult> AddSalaryRecord(int id, [FromBody] SalaryRecordCreateDto dto)
        {
            var record = new SalaryRecord
            {
                Amount = dto.Amount,
                EffectiveDate = dto.EffectiveDate,
                Notes = dto.Notes
            };

            await _repository.AddSalaryRecordAsync(id, record);
            return Ok(new { message = "Salary record added successfully." });
        }
    }
}
