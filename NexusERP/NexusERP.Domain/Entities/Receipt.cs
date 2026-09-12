using NexusERP.Application.Interfaces;
using NexusERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Entities
{
    public class Receipt : IAuditTracked
    {
        public int ReceiptId { get; set; }
        public string ReceiptNumber { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

        public int ShiftId { get; set; }
        public Shift? Shift { get; set; }

        public int StoreId { get; set; }
        public Store? Store { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public decimal SubTotal { get; set; }
        public decimal CartDiscountAmount { get; set; }
        public decimal TotalVatAmount { get; set; }
        public decimal FinalTotal { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public ICollection<ReceiptItem> Lines { get; set; } = new List<ReceiptItem>();

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
