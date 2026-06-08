using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces
{
    public interface ICreationTracked
    {
        DateTime CreatedAt { get; set; }
    }
    public interface IAuditTracked : ICreationTracked
    {
        DateTime? UpdatedAt { get; set; }
    }
}
