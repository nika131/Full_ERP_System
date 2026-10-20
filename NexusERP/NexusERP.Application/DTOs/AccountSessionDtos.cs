
using System.ComponentModel.DataAnnotations;

namespace NexusERP.Application.DTOs
{
    public class AccountLoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshAccountSessionDto
    {
        [Required, StringLength(256, MinimumLength = 20)]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class AccountSessionResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}
