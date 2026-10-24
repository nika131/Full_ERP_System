
using NexusERP.Application.DTOs;

namespace NexusERP.Application.Interfaces.Services
{
    public interface ICompanyAccessService
    {
        Task<List<MyCompanyDto>> GetMyCompaniesAsync(int accountId);
        Task<SelectCompanyResponseDto> SelectCompanyAsync(int accountId, Guid sessionId, int companyId);
    }
}
