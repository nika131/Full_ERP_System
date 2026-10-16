using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusERP.Api.Extensions;
using NexusERP.Application.DTOs;
using NexusERP.Domain.Entities;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("Api/[Controller]")]
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> GetSettings(string key)
        {
            int companyId = User.GetCurrentCompanyId();

            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s =>
                s.CompanyId == companyId &&
                s.SettingKey == key
            );

            if (setting == null)
                return NotFound(new { message = "Setting not found." });

            return Ok(new
            {
                key = setting.SettingKey,
                value = setting.SettingValue,
                description = setting.Description
            });
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSettingDto dto)
        {
            int companyId = User.GetCurrentCompanyId();
            int userId = User.GetCurrentUserId();

            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s =>
                s.CompanyId == companyId &&
                s.SettingKey == key
            );

            if (setting == null)
                return NotFound(new { message = "Setting not found." });

            if (key == "GlobalLowStockThreshold" && !int.TryParse(dto.Value, out _))
                return BadRequest(new { message = "Global Low Stock Threshold must be a valid number." });

            if (key == "AllowNegativeInventory" && !bool.TryParse(dto.Value, out _))
                return BadRequest(new { message = "Allow Negative Inventory must be 'true' or 'false'." });

            if (key == "DiscountPolicy" && dto.Value != "Enabled" && dto.Value != "Disabled" && dto.Value != "AdminOnly")
                return BadRequest(new { message = "Discount Policy must be 'Enabled', 'Disabled', or 'AdminOnly'." });

            var oldValue = setting.SettingValue;

            setting.SettingValue = dto.Value;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedByUserId = userId;

            _context.SystemAuditLogs.Add(new SystemAuditLog
            {
                CompanyId = companyId,
                UserId = userId,
                EntityType = "SystemSetting",
                EntityId = 0,
                Action = "Update",
                ChangesMade = $"Changed setting '{key}' from '{oldValue}' to '{dto.Value}'"
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Setting updated successfully." });
        }
    }
}