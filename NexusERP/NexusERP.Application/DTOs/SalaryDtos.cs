using System.ComponentModel.DataAnnotations;

namespace NexusERP.Application.DTOs
{
    public class SalaryDtos 
    {
        public class SalaryRecordResponseDto
        {
            public int SalaryRecordId { get; set; }
            public decimal Amount { get; set; }
            public DateTime EffectiveDate { get; set; }
            public string? Notes { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class SalaryRecordCreateDto
        {
            [Range(typeof(decimal), "0.01", "1000000")]
            public decimal Amount { get; set; }

            public DateTime EffectiveDate { get; set; }
            
            [StringLength(200)]
            public string? Notes { get; set; }
        }
    }
}
