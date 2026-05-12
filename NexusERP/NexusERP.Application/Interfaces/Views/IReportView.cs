using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Views
{
    public interface IReportView
    {
        IEnumerable<InventoryTransaction> GridDataSource { set; }
        void ShowMessage(string message);
    }
}
