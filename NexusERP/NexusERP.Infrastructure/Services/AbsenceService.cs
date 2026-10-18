using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Exceptions;

namespace NexusERP.Infrastructure.Services
{
    public class AbsenceService : IAbsenceService
    {
        private readonly IAbsenceRepository _absenceRepository;

        public AbsenceService(IAbsenceRepository absenceRepository)
        {
            _absenceRepository = absenceRepository;
        }

        public async Task<UserAbsence> SubmitRequestAsync(int companyId, int userId, UserAbsence absence)
        {
            if (absence.EndDate < absence.StartDate)
                throw new AppException("End date cannot be before start date.");

            bool hasOverlap = await _absenceRepository.HasOverlappingAbsenceAsync(companyId, userId, absence.StartDate, absence.EndDate);

            if (hasOverlap)
                throw new AppException("You already have an active or pending absence during this timeframe.");

            absence.CompanyId = companyId;
            absence.UserId = userId;
            absence.Status = AbsenceStatus.Pending;

            return await _absenceRepository.SubmitRequestAsync(companyId, absence);
        }

        public async Task ReviewRequestAsync(int companyId, int absenceId, int reviewerId, string status, string? comments)
        {
            var absence = await _absenceRepository.GetByIdAsync(companyId, absenceId);

            if (absence == null)
                throw new AppException("Leave request not found.");

            if (absence.Status != AbsenceStatus.Pending)
                throw new AppException("This request has already been processed.");

            if (!Enum.TryParse<AbsenceStatus>(status, true, out var parsedStatus) || parsedStatus == AbsenceStatus.Pending)
                throw new AppException("Invalid review status. Must be 'Approved' or 'Rejected'.");

            absence.Status = parsedStatus;
            absence.ReviewedByUserId = reviewerId;
            absence.ReviewerComments = comments;

            await _absenceRepository.ReviewRequestAsync(companyId, absence);
        }
    }
}