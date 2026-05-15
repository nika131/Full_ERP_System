using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Views
{
    public interface ILoginView
    {
        string Username { get; }
        string Password { get; }

        void ShowMessage(string message);
        void ShowError(string message);
        
        void HideView();
        void OpenMainDashboard();
    }
}
