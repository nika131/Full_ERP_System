using NexusERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IAuditRepository
    {
        IEnumerable<SystemAuditLog> GetAll();

        IEnumerable<SystemAuditLog> SearchLogs(string keyword);
    }
}
