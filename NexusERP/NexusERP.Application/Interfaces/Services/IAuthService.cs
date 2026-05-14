using NexusERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Services
{
    public interface IAuthService
    {
        void Register(string fullname, string username, string plaintextPassword, UserRole role);
        string Login(string username, string password);
    }
}
