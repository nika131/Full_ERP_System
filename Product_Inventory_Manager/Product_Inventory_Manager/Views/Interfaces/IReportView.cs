using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product_Inventory_Manager.Product_Inventory_Manager.Views.Interfaces
{
    internal interface IReportView
    {
        DataTable GridDataSource { set; }
        void ShowMessage(string message);
    }
}
