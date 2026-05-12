using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Views
{
    public interface IDashboardView
    {
        string TotalValue { set; }
        string TotalProfitValue { set; }
        string MarginValue { set; }
        String LowStrockCount { set; }
        string InventoryHealth { set; }
    }
}
