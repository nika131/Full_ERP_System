using NexusERP.Application.Interfaces.Services;
using NexusERP.Application.Interfaces.Views;
using NexusERP.Domain.Enums;
using NexusERP.Domain.State;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Presenters
{
    public class LoginPresenter
    {
        private ILoginView _view = null!;
        private readonly IAuthService _authService;

        public LoginPresenter(IAuthService authService)
        {
            _authService = authService;
        }

        public void SetView(ILoginView view)
        {
            _view = view;
        }

        public void Login()
        {
            if (string.IsNullOrWhiteSpace(_view.Username) || string.IsNullOrWhiteSpace(_view.Password))
            {
                _view.ShowError("Username and Password are required.");
                return;
            }

            try
            {
                string token = _authService.Login(_view.Username, _view.Password);

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                string roleClaim = jwtToken.Claims.First(c => c.Type == "role").Value;
                string usernameClaim = jwtToken.Claims.First(c => c.Type == "unique_name").Value;
                string FullNameClaim = jwtToken.Claims.First(c => c.Type == "given_name").Value;
                string idClaim = jwtToken.Claims.First(c => c.Type == "newidentityfier").Value;

                UserSession.JwtToken = token;
                UserSession.UserId = int.Parse(idClaim);
                UserSession.FullName = FullNameClaim;
                UserSession.UserName = usernameClaim;
                UserSession.Role = Enum.Parse<UserRole>(roleClaim);

                _view.HideView();
                _view.OpenMainDashboard();
            }
            catch (Exception ex)
            {
                _view.ShowError(ex.Message);
            }
        }
    }
}
