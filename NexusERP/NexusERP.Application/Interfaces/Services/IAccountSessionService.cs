
using NexusERP.Application.DTOs;

namespace NexusERP.Application.Interfaces.Services
{
    public interface IAccountSessionService
    {
        Task<AccountSessionResponseDto> LoginAsync(AccountLoginDto dto);
        Task<AccountSessionResponseDto> RefreshAsync(RefreshAccountSessionDto dto);
        Task LogoutAsync(int accountId, Guid sessionId);
    }
}
