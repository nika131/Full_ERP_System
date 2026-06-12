using NexusERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Services
{
    public interface IAbsenceService
    {
        Task<UserAbsence> SubmitRequestAsync(int userId, UserAbsence absence);
        Task ReviewRequestAsync(int absenceId, int reviewerId, string status, string? comments);
    }
}
