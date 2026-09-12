using NexusERP.Application.Interfaces;
using NexusERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Entities
{
    public class Shift : IAuditTracked
    {
        public int ShiftId { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int StoreId { get; set; }
        public Store? Store { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public decimal StartingCash { get; set; }
        public decimal ExpectedEndingCash { get; set; } = 0;
        public decimal? ActualEndingCash { get; set; }

        public decimal TotalSales { get; set; } = 0;
        public decimal TotalProfit { get; set; } = 0;

        public ShiftStatus Status { get; set; } = ShiftStatus.Open;
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
