using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.DTOs
{
    public class DashboardFilterRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? StartHour { get; set; }
        public TimeSpan? EndHour { get; set; }

        public List<int>? StoreIds { get; set; }
        public List<int>? CategoryIds { get; set; }
        public List<int>? SupplierIds { get; set; }
        public List<int>? EmployeeIds { get; set; }
    }
}
