using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IAbsenceRepository
    {
        Task<UserAbsence> SubmitRequestAsync(int userId, UserAbsence absence);
        Task ReviewRequestAsync(int absenceId, int reviewerId, string status, string? comments);
        Task<IEnumerable<UserAbsence>> GetMyAbsencesAsync(int userId);
        Task<IEnumerable<UserAbsence>> GetPendingRequestsAsync();
    }
}
