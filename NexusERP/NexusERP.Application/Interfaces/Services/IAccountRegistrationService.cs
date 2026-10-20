using NexusERP.Application.DTOs;

namespace NexusERP.Application.Interfaces.Services
{
    public interface IAccountRegistrationService
    {
        Task RegisterAsync(RegisterAccountDto dto);
        Task VerifyEmailAsync(VerifyAccountEmailDto dto);
        Task ResendCodeAsync(ResendAccountCodeDto dto);
    }
}