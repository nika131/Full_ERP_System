using NexusERP.Application.Interfaces;
using NexusERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Entities
{
    public class CashMovement : IAuditTracked
    {
        public int MovementId { get; set; }

        public int ShiftId { get; set; }
        public Shift? Shift { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int? StoreId { get; set; }
        public Store? Store { get; set; }

        public CashMovementType MovementType { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
