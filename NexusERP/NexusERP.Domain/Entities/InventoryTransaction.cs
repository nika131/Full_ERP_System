using NexusERP.Application.Interfaces;
using NexusERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Entities
{
    public class InventoryTransaction
    {
        public int TransactionId { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
        
        public TransactionAction TransactionType { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Profit { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
