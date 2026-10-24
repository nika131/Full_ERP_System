using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Entities
{
    public class SystemSetting
    {
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        public DateTime UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }
    }
}
