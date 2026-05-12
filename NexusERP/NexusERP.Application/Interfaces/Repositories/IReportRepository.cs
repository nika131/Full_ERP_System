using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IReportRepository
    {
        IEnumerable<InventoryTransaction> GetAll();
        IEnumerable<InventoryTransaction> Search(string Keyword);
    }
}
