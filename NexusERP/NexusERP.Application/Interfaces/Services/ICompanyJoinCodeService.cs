
using NexusERP.Application.DTOs;

namespace NexusERP.Application.Interfaces.Services
{
    public interface ICompanyJoinCodeService
    {
        Task<RotateCompanyJoinCodeResponseDto> RotateAsync(int companyId, int actorUserId);
    }
}
