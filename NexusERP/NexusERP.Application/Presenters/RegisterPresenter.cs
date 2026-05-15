using NexusERP.Application.Interfaces.Services;
using NexusERP.Application.Interfaces.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Presenters
{
    public class RegisterPresenter
    {
        private IRegisterView _view = null;
        private readonly IAuthService _authService;

        public RegisterPresenter(IAuthService authService)
        {
            _authService = authService;
        }

        public void SetView(IRegisterView view)
        {
            _view = view;
        }

        public void Register()
        {
            if(string.IsNullOrWhiteSpace(_view.Username) || string.IsNullOrWhiteSpace(_view.Username) || string.IsNullOrEmpty(_view.FullName))
            {
                _view.ShowError("Username, Password and Fullname is required.");
                return;
            }

            try
            {
                _authService.Register(_view.FullName, _view.Username, _view.Password, _view.SelectRole);

                _view.ShowMessage("User registered successfully! Please log in.");
            }
            catch (Exception ex)
            {
                _view.ShowError(ex.Message);
            }
        }
    }
}
