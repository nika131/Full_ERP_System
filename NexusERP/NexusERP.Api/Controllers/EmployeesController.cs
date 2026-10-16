using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.Extensions;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using static NexusERP.Application.DTOs.SalaryDtos;

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
        public async Task<IActionResult> GetEmployees([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, [FromQuery] string roleFilter = "All")
        {
            if (pageSize > 100) pageSize = 100;

            int companyId = User.GetCurrentCompanyId();

            var result = await _repository.GetPagedAsync(companyId, page, pageSize, searchTerm, roleFilter);

            var responseData = result.Items.Select(u => new EmployeeResponseDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Username = u.Username,
                RoleId = u.RoleId,
                RoleName = u.Role?.Name ?? "Unassigned",
                PosPin = u.PosPin,
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
            int companyId = User.GetCurrentCompanyId();

            if (id == currentUserId)
                return BadRequest(new { message = "Security Lock: You cannot remove your own Admin privileges." });

            var userToUpdate = new User
            {
                UserId = id,
                FullName = dto.FullName,
                Username = dto.Username,
                RoleId = dto.RoleId,
                PosPin = dto.PosPin
            };

            await _repository.UpdateUser(userToUpdate, currentUserId, companyId);

            return Ok(new { message = "Employee updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            int currentUserId = User.GetCurrentUserId();
            int companyId = User.GetCurrentCompanyId();

            if (id == currentUserId)
                return BadRequest(new { message = "Security Lock: You cannot delete your own active account." });

            await _repository.DeleteUser(id, currentUserId, companyId);

            return Ok(new { message = "Employee access revoked successfully." });
        }

        [HttpGet("{id}/salary")]
        public async Task<IActionResult> GetSalaryHistory(int id)
        {
            int companyId = User.GetCurrentCompanyId();

            var history = await _repository.GetSalaryHistoryAsync(companyId, id);

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

        [HttpGet("mysalary")]
        public async Task<IActionResult> GetMySalaryHistory()
        {
            int currentUserId = User.GetCurrentUserId();
            int companyId = User.GetCurrentCompanyId();

            var history = await _repository.GetSalaryHistoryAsync(companyId, currentUserId);

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
            int currentUserId = User.GetCurrentUserId();
            int companyId = User.GetCurrentCompanyId();

            if (id == currentUserId)
                return BadRequest(new { message = "Security Lock: You cannot set your own salary." });

            var record = new SalaryRecord
            {
                Amount = dto.Amount,
                EffectiveDate = dto.EffectiveDate,
                Notes = dto.Notes
            };

            await _repository.AddSalaryRecordAsync(companyId, id, record);

            return Ok(new { message = "Salary record added successfully." });
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetLookupUsers()
        {
            int companyId = User.GetCurrentCompanyId();

            var users = await _repository.GetLookupUsersAsync(companyId);

            return Ok(users);
        }
    }
}