using NexusERP.Application.Interfaces;
using NexusERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Entities
{
    public class Product : IAuditTracked
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [ConcurrencyCheck]
        public int Quantity { get; set; }
        public int? LowStockThreshold { get; set; }

        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }

        public decimal VatRate { get; set; }
        public decimal MarketDiscountRate { get; set; }
        public decimal MaxDiscountPercentage { get; set; }

        public string? Barcode { get; set; }

        public string? ImageUrl { get; set; }
        public ShapeType? ShapeType { get; set; }
        public string? ShapeColor { get; set; }
        public string? ShapeText { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
