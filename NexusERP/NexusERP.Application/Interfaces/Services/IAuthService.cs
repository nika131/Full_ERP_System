using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task Register(
            string fullname,
            string username,
            string plaintextPassword,
            int roleId,
            int actorUserId,
            int companyId,
            string posPin
        );

        Task<string> Login(string username, string plaintextPassword);
        string GenerateJwtToken(User user);
        Task<string> LoginWithPin(int companyId, int targetUserId, string pin);
    }
}