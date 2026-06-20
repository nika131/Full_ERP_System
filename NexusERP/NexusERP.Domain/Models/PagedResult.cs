using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NexusERP.Domain.Models
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class CursorPagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public DateTime? NextCreatedAt { get; set; }
        public int? NextId { get; set; }
        public int PageSize { get; set; }
        public bool HasMorePages => NextCreatedAt.HasValue && NextLogId.HasValue;

    }
}
