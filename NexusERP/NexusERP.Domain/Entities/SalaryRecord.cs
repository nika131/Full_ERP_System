using NexusERP.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Entities
{
    public class SalaryRecord : IAuditTracked
    {
        public int SalaryRecordId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public decimal Amount { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
