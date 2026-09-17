using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.Extensions;
using NexusERP.Application.DTOs;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("Api/[Controller]")]
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
            var setting = await _context.SystemSettings.FindAsync(key);
            if (setting == null) return NotFound(new { message = "Setting not found. " });

            return Ok( new { key = setting.SettingKey, value = setting.SettingValue, description = setting.Description });
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSettingDto dto)
        {
            var setting = await _context.SystemSettings.FindAsync(key);
            if (setting == null) return NotFound(new { message = "Setting not found. " });

            if (key == "GlobalLowStockThreshold" && !int.TryParse(dto.Value, out _))
            {
                return BadRequest(new { message = "Global Low Stock Threshold must be valid number. " });
            }

            if (key == "AllowNegativeInventory" && !bool.TryParse(dto.Value, out _))
            {
                return BadRequest(new { message = "Allow Negative Inventory must be 'true' or 'false'." });
            }

            if (key == "DiscountPolicy" && dto.Value != "Enabled" && dto.Value != "Disabled" && dto.Value != "AdminOnly")
            {
                return BadRequest(new { message = "Discount Policy must be 'Enabled', 'Disabled', or 'AdminOnly'." });
            }

            var oldVal = setting.SettingValue;
            setting.SettingValue = dto.Value;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedByUserId = User.GetCurrentUserId();

            _context.SystemAuditLogs.Add(new Domain.Entities.SystemAuditLog
            {
                UserId = User.GetCurrentUserId(),
                EntityType = "SystemSetting",
                EntityId = 0,
                Action = "update",
                ChangesMade = $"Changed setting '{key}' from '{oldVal}' to '{dto.Value}'",
                CreatedAt = DateTime.UtcNow,
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Setting updated successfully." });
        }
    }
}
