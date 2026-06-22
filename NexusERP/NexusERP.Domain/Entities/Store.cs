using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace NexusERP.Domain.Entities
{
    public class Store
    {
        public int StoreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public Point Location { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; }

        public ICollection<InventoryTransaction> Transactions { get; set; }
    }
}
