
namespace NexusERP.Application.DTOs
{
    public class MyCompanyDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    public class SelectCompanyResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
    }
}
