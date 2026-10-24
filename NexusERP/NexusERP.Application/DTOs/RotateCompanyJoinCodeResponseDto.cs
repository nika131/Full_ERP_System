
namespace NexusERP.Application.DTOs
{
    public class RotateCompanyJoinCodeResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
