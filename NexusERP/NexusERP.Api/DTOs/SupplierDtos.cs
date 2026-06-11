using System.ComponentModel.DataAnnotations;

namespace NexusERP.Api.DTOs
{
    public class SupplierUpsertDto
    {
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Company Name is required.")]
        public string CompanyName { get; set; } = string.Empty;

        public string? ContactName {  get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    public class SupplierResponseDto
    {
        public int SupplierId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    public class SupplierLookupDto
    {
        public int SupplierId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }
}
