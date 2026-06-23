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
    }

}
