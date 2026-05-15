using NexusERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Views
{
    public interface IRegisterView
    {
        string FullName { get; }
        string Username { get; }
        string Password { get; }
        UserRole SelectRole { get; }

        void ShowMessage(string message);
        void ShowError(string message);
    }
}
