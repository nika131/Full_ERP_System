using System.ComponentModel.DataAnnotations;

namespace NexusERP.Application.DTOs
{
    public class RegisterAccountDto
    {
        [Required, EmailAddress, StringLength(320)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8), MaxLength(128)]
        public string Password { get; set; } = string.Empty;
    }

    public class VerifyAccountEmailDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, RegularExpression(@"^[0-9]{6}$")]
        public string Code { get; set; } = string.Empty;
    }

    public class ResendAccountCodeDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}