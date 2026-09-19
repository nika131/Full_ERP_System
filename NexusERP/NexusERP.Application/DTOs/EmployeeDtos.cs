using Azure.Core.Pipeline;
using System.ComponentModel.DataAnnotations;

namespace NexusERP.Application.DTOs
{
    public class EmployeeResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string? PosPin { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class EmployeeUpdateDto
    {
        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; } = String.Empty;

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        public int RoleId { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN must be exactly 4 digits.")]
        public string PosPin { get; set; } = string.Empty;
    }

    public class UserLookupDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string Username { get; set; } = string.Empty;
    }

}
