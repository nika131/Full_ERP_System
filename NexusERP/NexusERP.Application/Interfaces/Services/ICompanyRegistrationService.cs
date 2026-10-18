using NexusERP.Application.DTOs;

namespace NexusERP.Application.Interfaces.Services
{
    public interface ICompanyRegistrationService
    {
        Task<CreateCompanyResultDto> CreateCompanyAsync(int accountId, CreateCompanyRequestDto dto);
    }
}