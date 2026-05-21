using NexusERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Views
{
    public interface IEmployeeView
    {
        IEnumerable<User> GridDataSource { set; }
        void ShowMessage(string message);
        void ShowError(string message);
    }
}
