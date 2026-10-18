using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Services
{
    public interface IAbsenceService
    {
        Task<UserAbsence> SubmitRequestAsync(int companyId, int userId, UserAbsence absence);
        Task ReviewRequestAsync(int companyId, int absenceId, int reviewerId, string status, string? comments);
    }
}