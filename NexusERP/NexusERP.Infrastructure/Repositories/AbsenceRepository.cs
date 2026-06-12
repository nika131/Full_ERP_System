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

        public async Task<UserAbsence> SubmitRequestAsync(UserAbsence absence)
        {
            await _context.UserAbsences.AddAsync(absence);
            await _context.SaveChangesAsync();
            return absence;
        }

        public async Task ReviewRequestAsync(UserAbsence absence)
        {
            await _context.SaveChangesAsync();
        }

        public async Task<UserAbsence?> GetByIdAsync(int absenceId)
        {
            return await _context.UserAbsences.FindAsync(absenceId);
        }

        public async Task<IEnumerable<UserAbsence>> GetMyAbsencesAsync(int userId)
        {
            return await _context.UserAbsences
                .Include(a => a.User)
                .Include(a => a.ReviewedBy)
                .Where(a => a.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserAbsence>> GetPendingRequestsAsync()
        {
            return await _context.UserAbsences
                .Include(a => a.User)
                .Where(a => a.Status == AbsenceStatus.Pending)
                .AsNoTracking()
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasOverlappingAbsenceAsync(int userId, DateTime start, DateTime end)
        {
            return await _context.UserAbsences
                .AnyAsync(a =>
                    a.UserId == userId &&
                    a.Status != AbsenceStatus.Rejected && 
                    a.StartDate <= end &&
                    a.EndDate >= start);
        }
    }
}
