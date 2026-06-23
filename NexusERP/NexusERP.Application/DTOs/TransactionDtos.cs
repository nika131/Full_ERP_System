using System.ComponentModel.DataAnnotations;

namespace NexusERP.Application.DTOs
{
    public class TransactionResponseDto
    {
        public int TransactionId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string TransactionType {  get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Profit { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TransactionRequestDto
    {
        [Required]
        public string TransactionType { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        public int SupplierId { get; set; }
        public int Quantity { get; set; }

        public decimal ProductPrice { get; set; }
        public decimal CostPrice { get; set; }
    }
}
