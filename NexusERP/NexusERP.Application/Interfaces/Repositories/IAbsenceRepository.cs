using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IAbsenceRepository
    {
        Task<UserAbsence> SubmitRequestAsync(UserAbsence absence);
        Task ReviewRequestAsync(UserAbsence absence);
        Task<UserAbsence?> GetByIdAsync(int absenceId);
        Task<IEnumerable<UserAbsence>> GetMyAbsencesAsync(int userId);
        Task<IEnumerable<UserAbsence>> GetPendingRequestsAsync();
        Task<bool> HasOverlappingAbsenceAsync(int userId, DateTime start, DateTime end);
    }
}
