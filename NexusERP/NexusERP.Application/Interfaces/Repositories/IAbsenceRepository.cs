using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IAbsenceRepository
    {
        Task<UserAbsence> SubmitRequestAsync(int companyId, UserAbsence absence);
        Task ReviewRequestAsync(int companyId, UserAbsence absence);
        Task<UserAbsence?> GetByIdAsync(int companyId, int absenceId);
        Task<IEnumerable<UserAbsence>> GetMyAbsencesAsync(int companyId, int userId);
        Task<IEnumerable<UserAbsence>> GetPendingRequestsAsync(int companyId);
        Task<bool> HasOverlappingAbsenceAsync(int companyId, int userId, DateTime start, DateTime end);
    }
}