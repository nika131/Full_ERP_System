using System.ComponentModel.DataAnnotations;

namespace NexusERP.Application.DTOs
{
    public class CreateCompanyRequestDto
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string CompanyName { get; set; } = string.Empty;

        [Required, StringLength(150, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 2)]
        public string Username { get; set; } = string.Empty;

        public string? PosPin { get; set; }
    }

    public class CreateCompanyResultDto
    {
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string JoinCode { get; set; } = string.Empty;
    }
}