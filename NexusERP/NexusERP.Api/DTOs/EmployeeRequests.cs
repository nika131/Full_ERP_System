using System.ComponentModel.DataAnnotations;

namespace NexusERP.Api.DTOs
{
    public class EmployeeResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EmployeeUpdateDto
    {
        [Required]
        public string FullName { get; set; } = String.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
