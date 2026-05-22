using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace NexusERP.Api.DTOs
{
    public class ProductUpsertDto
    {
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
        public int CategryId { get; set; }

        public int Quantity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        public decimal CostPrice { get; set; }
        public int SupplierId { get; set; }
    }

    public class TransactionRequestDto
    {
        [Required]
        public string TransactionType { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        public int SupplierId { get; set; }
        public int SoldQty { get; set; }

        public decimal ProductPrice { get; set; }
        public decimal CostPrice { get; set; }
    }
}
