using NexusERP.Application.Interfaces;
using NexusERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Entities
{
    public class UserAbsence : IAuditTracked
    {
        public int AbsenceId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public AbsenceType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Notes { get; set; }

        public AbsenceStatus Status { get; set; } = AbsenceStatus.Pending;

        public int? ReviewedByUserId { get; set; }
        public User? ReviewedBy { get; set; }
        public string? ReviewerComments { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
