using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Infrastructure.Repositories
{
    public class AbsenceRepository : IAbsenceRepository
    {
        private readonly ApplicationDbContext _context;

        public AbsenceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserAbsence> SubmitRequestAsync(int userId, UserAbsence absence)
        {
            if (absence.EndDate < absence.StartDate)
                throw new AppException("End date cannot be before start date.");

            absence.UserId = userId;
            absence.Status = AbsenceStatus.Pending;

            await _context.UserAbsences.AddAsync(absence);
            await _context.SaveChangesAsync();
            return absence;
        }

        public async Task ReviewRequestAsync(int absenceId, int reviewerId, string status, string? comments)
        {
            var absence = await _context.UserAbsences.FindAsync(absenceId);
            if (absence == null) throw new AppException("Leave request not found.");

            if (absence.Status != AbsenceStatus.Pending)
                throw new AppException("This request has already been processed.");

            if (!Enum.TryParse<AbsenceStatus>(status, true, out var parsedStatus) || parsedStatus == AbsenceStatus.Pending)
                throw new AppException("Invalid review status. Must be 'Approved' or 'Rejected'.");

            absence.Status = parsedStatus;
            absence.ReviewedByUserId = reviewerId;
            absence.ReviewerComments = comments;

            _context.UserAbsences.Update(absence);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserAbsence>> GetMyAbsencesAsync(int userId)
        {
            return await _context.UserAbsences
                .Include(a => a.User)
                .Include(a => a.ReviewedBy)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserAbsence>> GetPendingRequestsAsync()
        {
            return await _context.UserAbsences
                .Include(a => a.User)
                .Where(a => a.Status == AbsenceStatus.Pending)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}
