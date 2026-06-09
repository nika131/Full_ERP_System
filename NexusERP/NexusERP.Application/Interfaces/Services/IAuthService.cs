using NexusERP.Domain.Entities;
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
        void Register(string fullname, string username, string plaintextPassword, int roleId);
        string Login(string username, string password);
        string GenerateJwtToken(User user);
    }
}
