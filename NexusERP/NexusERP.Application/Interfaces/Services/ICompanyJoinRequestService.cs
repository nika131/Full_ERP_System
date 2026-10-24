
using NexusERP.Application.DTOs;

namespace NexusERP.Application.Interfaces.Services
{
    public interface ICompanyJoinRequestService
    {
        Task<int> SubmitAsync(int accountId, SubmitCompanyJoinRequestDto dto);
        Task<List<CompanyJoinRequestDto>> GetMyRequestsAsync(int accountId);
        Task<List<CompanyJoinRequestDto>> GetPendingRequestsAsync(int companyId, int reviewerUserId);
        Task ApproveAsync(int companyId, int reviewerUserId, int requestId, int roleId);
        Task RejectAsync(int companyId, int reviewerUserId, int requestId);
    }
}
