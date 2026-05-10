using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product_Inventory_Manager.Product_Inventory_Manager.Repositories
{
    internal interface IReportRepository
    {
        DataTable GetAll();
        DataTable Search(string Keyword);
    }
}
