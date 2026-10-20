
using System.ComponentModel.DataAnnotations;
using NexusERP.Domain.Enums;

namespace NexusERP.Application.DTOs
{
    public class SubmitCompanyJoinRequestDto
    {
        [Required]
        [RegularExpression(@"^[A-Fa-f0-9]{32}$", ErrorMessage = "Invalid company join code.")]
        public string JoinCode { get; set; } = string.Empty;

        [Required, StringLength(150, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 2)]
        public string Username { get; set; } = string.Empty;
    }

    public class ApproveCompanyJoinRequestDto
    {
        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }
    }

    public class CompanyJoinRequestDto
    {
        public int JoinRequestId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string AccountEmail { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public CompanyJoinRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? ApprovedRoleId { get; set; }
    }
}
