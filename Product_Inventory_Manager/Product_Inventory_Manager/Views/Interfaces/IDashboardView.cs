using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product_Inventory_Manager.Product_Inventory_Manager.Views.Interfaces
{
    internal interface IDashboardView
    {
        string totalValue { set; }
        string totalProfitValue { set; }
        string marginValue { set; }
        String lowStrockCount { set; }
        string inventoryHealth { set; }
        System.Drawing.Color healthColor { set; }
    }
}
