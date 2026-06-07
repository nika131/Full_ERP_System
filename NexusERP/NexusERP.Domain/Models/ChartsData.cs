using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Models
{
    public class RevenueChartData
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
    }

    public class TopProductChartData
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }
}
