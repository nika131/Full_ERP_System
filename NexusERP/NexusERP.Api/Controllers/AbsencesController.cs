using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.Extensions;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Exceptions;
using static NexusERP.Api.DTOs.AbsenceDtos;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AbsencesController : Controller
    {
        private readonly IAbsenceRepository _repository;

        public AbsencesController(IAbsenceRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestLeave([FromBody] LeaveRequestDto dto)
        {
            if (!Enum.TryParse<AbsenceType>(dto.Type, true, out var type))
                throw new AppException("Invalid absence type.");

            var absence = new UserAbsence
            {
                Type = type,
                StartDate = dto.StartDate.Date, 
                EndDate = dto.EndDate.Date,
                Notes = dto.Notes
            };

            await _repository.SubmitRequestAsync(User.GetCurrentUserId(), absence);
            return Ok(new { message = "Leave request submitted successfully." });
        }

        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyHistory()
        {
            var absences = await _repository.GetMyAbsencesAsync(User.GetCurrentUserId());
            return Ok(absences.Select(MapToResponseDto));
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var absences = await _repository.GetPendingRequestsAsync();
            return Ok(absences.Select(MapToResponseDto));
        }

        [HttpPut("{id}/review")]
        [Authorize(Policy = "Absences.Manage")] 
        public async Task<IActionResult> ReviewLeave(int id, [FromBody] LeaveReviewDto dto)
        {
            await _repository.ReviewRequestAsync(id, User.GetCurrentUserId(), dto.Status, dto.ReviewerComments);
            return Ok(new { message = $"Leave request {dto.Status.ToLower()}." });
        }

        private static LeaveResponseDto MapToResponseDto(UserAbsence a)
        {
            return new LeaveResponseDto
            {
                AbsenceId = a.AbsenceId,
                UserId = a.UserId,
                EmployeeName = a.User?.FullName ?? "Unknown",
                Type = a.Type.ToString(),
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Notes = a.Notes,
                Status = a.Status.ToString(),
                ReviewerName = a.ReviewedBy?.FullName,
                ReviewerComments = a.ReviewerComments,
                CreatedAt = a.CreatedAt
            };
        }
    }
}
