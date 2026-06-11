namespace NexusERP.Api.DTOs
{
    public class AbsenceDtos
    {
        public class LeaveRequestDto
        {
            public string Type { get; set; } = string.Empty; 
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string? Notes { get; set; }
        }

        public class LeaveReviewDto
        {
            public string Status { get; set; } = string.Empty; 
            public string? ReviewerComments { get; set; }
        }

        public class LeaveResponseDto
        {
            public int AbsenceId { get; set; }
            public int UserId { get; set; }
            public string EmployeeName { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string? Notes { get; set; }
            public string Status { get; set; } = string.Empty;
            public string? ReviewerName { get; set; }
            public string? ReviewerComments { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
