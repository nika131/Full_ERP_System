using System.ComponentModel.DataAnnotations;

namespace NexusERP.Application.DTOs
{
    public class RegisterRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }
        [Required]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN must be exactly 4 digits.")]
        public string PosPin { get; set; } = string.Empty;
    }
}
