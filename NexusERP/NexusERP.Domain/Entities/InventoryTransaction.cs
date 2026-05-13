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
        public string ProductName { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public TransactionType TransactionType { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public decimal Profit { get; set; }
        public DateTime TransactionDate { get; set; }

    }
}
