using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Entities
{
    public class ReceiptItem
    {
        public int ItemId { get; set; }

        public int ReceiptId { get; set; }
        public Receipt? Receipt { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal CostPrice { get; set; }

        public decimal VatRate { get; set; }
        public decimal MarketDiscountAmount { get; set; }
        public decimal ManualItemDiscountAmount { get; set; }

        public decimal LineTotal { get; set; }
    }
}
