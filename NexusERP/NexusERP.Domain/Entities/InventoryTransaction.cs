using NexusERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Entities
{
    public class InventoryTransaction
    {
        public int TransactionId { get; set; }
        public int ProductId { get; set; }
        public int? SupplierId { get; set; }
        public int UserId { get; set; }
        public TransactionAction TransactionType { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount {  get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal Profit { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
