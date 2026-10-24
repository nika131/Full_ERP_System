using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Infrastructure.Repositories
{
    public class AbsenceRepository : IAbsenceRepository
    {
        private readonly ApplicationDbContext _context;

        public AbsenceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserAbsence> SubmitRequestAsync(int companyId, UserAbsence absence)
        {
            bool userExists = await _context.Users.AnyAsync(u => u.CompanyId == companyId && u.UserId == absence.UserId);

            if (!userExists)
                throw new AppException("Employee not found.");

            absence.CompanyId = companyId;

            await _context.UserAbsences.AddAsync(absence);
            await _context.SaveChangesAsync();

            return absence;
        }

        public async Task ReviewRequestAsync(int companyId, UserAbsence absence)
        {
            if (absence.CompanyId != companyId)
                throw new AppException("Leave request not found.");

            await _context.SaveChangesAsync();
        }

        public async Task<UserAbsence?> GetByIdAsync(int companyId, int absenceId)
        {
            return await _context.UserAbsences
                .FirstOrDefaultAsync(a => a.CompanyId == companyId && a.AbsenceId == absenceId);
        }

        public async Task<IEnumerable<UserAbsence>> GetMyAbsencesAsync(int companyId, int userId)
        {
            return await _context.UserAbsences
                .Include(a => a.User)
                .Include(a => a.ReviewedBy)
                .Where(a => a.CompanyId == companyId && a.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserAbsence>> GetPendingRequestsAsync(int companyId)
        {
            return await _context.UserAbsences
                .Include(a => a.User)
                .Where(a => a.CompanyId == companyId && a.Status == AbsenceStatus.Pending)
                .AsNoTracking()
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasOverlappingAbsenceAsync(int companyId, int userId, DateTime start, DateTime end)
        {
            return await _context.UserAbsences.AnyAsync(a =>
                a.CompanyId == companyId &&
                a.UserId == userId &&
                a.Status != AbsenceStatus.Rejected &&
                a.StartDate <= end &&
                a.EndDate >= start
            );
        }
    }
}