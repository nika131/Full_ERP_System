using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusERP.Api.Extensions;
using NexusERP.Domain.Constants;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Api.Controllers
{
    public class CreatePosEmployeeDto
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }

        [Required, RegularExpression("^[0-9]{4}$")]
        public string PosPin { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/employees/pos-only")]
    [Authorize(Policy = Permissions.CreateUsers)]
    public class PosOnlyEmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PosOnlyEmployeesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePosEmployeeDto dto)
        {
            int companyId = User.GetCurrentCompanyId();
            int actorId = User.GetCurrentUserId();

            if (!User.HasPermission(Permissions.AssignRoles))
                return StatusCode(403, new { message = "You do not have permission to assign employee roles." });

            string fullName = dto.FullName.Trim();
            string username = dto.Username.Trim();
            if (new[] { "0000", "1111", "1234", "4321", "2580", "1212" }.Contains(dto.PosPin) || dto.PosPin.Distinct().Count() == 1)
                return BadRequest(new { message = "Choose a less predictable four-digit PIN." });

            if (fullName.Length < 2 || username.Length < 3)
                return BadRequest(new { message = "Enter a valid full name and username." });

            var actor = await _db.Users.Include(u => u.Role).AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == actorId && u.CompanyId == companyId && u.IsActive && u.Role.IsActive);
            var role = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.RoleId == dto.RoleId && r.CompanyId == companyId && r.IsActive);
            if (actor == null || role == null || role.IsSystemRole ||
                role.Permissions.Except(actor.Role.Permissions).Any())
                return BadRequest(new { message = "Select an assignable non-system role belonging to your company." });

            if (!role.Permissions.Contains(Permissions.ViewPos))
                return BadRequest(new { message = "A POS-only employee must have the POS.View permission." });

            if (await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.CompanyId == companyId && u.Username == username))
                return Conflict(new { message = "Username already exists in this company." });

            var user = new User
            {
                CompanyId = companyId,
                AccountId = null, // No website sign-in or password reset account.
                FullName = fullName,
                Username = username,
                RoleId = role.RoleId,
                PosPin = BCrypt.Net.BCrypt.HashPassword(dto.PosPin),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Convert.ToHexString(RandomNumberGenerator.GetBytes(32))),
                IsActive = true
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            _db.SystemAuditLogs.Add(new SystemAuditLog
            {
                CompanyId = companyId,
                UserId = actorId,
                EntityType = "User",
                EntityId = user.UserId,
                Action = "CreatePosOnly",
                ChangesMade = $"Created POS-only employee '{username}' with role '{role.Name}'"
            });
            await _db.SaveChangesAsync();

            return StatusCode(201, new { message = "POS-only employee created. They can use their PIN on an activated company terminal, but cannot sign into the website.", userId = user.UserId });
        }
    }
}
