using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Services
{
    public interface IAuthService
    {
        string GenerateCompanyJwtToken(
            User user,
            int accountId,
            Guid sessionId,
            DateTime expiresAt
        );
    }
}