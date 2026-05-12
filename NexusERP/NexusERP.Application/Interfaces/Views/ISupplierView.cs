using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Domain.Entities;

namespace NexusERP.Application.Interfaces.Views
{
    public interface ISupplierView
    {
        IEnumerable<Supplier> SupplierGridDataSource { set; }
        int SupplierId { get; }
        string ViewCompanyName { get; }
        string ContactName { get; }
        string Phone { get; }
        string Email { get; }
    
        void ShowMessage(string message);
    }
}
