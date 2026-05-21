using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Views;
using NexusERP.Application.State;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Presenters
{
    public class EmployeePresenter
    {
        private readonly IUserRepository _repository;
        private IEmployeeView _view = null!;

        public EmployeePresenter(IUserRepository repository)
        {
            _repository = repository;
        }

        public void SetView(IEmployeeView view)
        {
            _view = view;
        }

        private bool IsAuthorized()
        {
            if (NexusERP.Domain.State.UserSession.Role != Domain.Enums.UserRole.Admin)
            {
                _view.ShowError("Security Violation: Only Administrators can perform this action.");
                return false;
            }
            return true;
        }

        public void LoadAndFilter(string keyword, string roleFilter)
        {
            if (!IsAuthorized()) return;

            try
            {
                IEnumerable<User> users;

                if (string.IsNullOrWhiteSpace(keyword))
                    users = _repository.GetAllUsers();
                else
                    users = _repository.SearchUsers(keyword);

                if (roleFilter != "All" && Enum.TryParse(roleFilter, out UserRole role))
                {
                    users = users.Where(u => u.Role == role);
                }

                _view.GridDataSource = users.ToList();
            }
            catch (Exception ex)
            {
                _view.ShowError("Error loading employees: " + ex.Message);
            }
        }

        public void UpdateEmployee(User updatedUser)
        {
            if (!IsAuthorized()) return;

            try
            {
                if (updatedUser.UserId == UserSession.UserId && updatedUser.Role != UserRole.Admin)
                {
                    _view.ShowError("Security Lock: You cannot remove your own Admin privileges.");
                    return;
                }

                _repository.UpdateUser(updatedUser);
                _view.ShowMessage("Employee updated successfully.");

                LoadAndFilter("", "All");
            }
            catch (Exception ex)
            {
                _view.ShowError("Error updating employee: " + ex.Message);
            }
        }

        public void DeleteEmployee(int userId)
        {
            if (!IsAuthorized()) return;
            try
            {
                if (userId == UserSession.UserId)
                {
                    _view.ShowError("Security Lock: You cannot delete your own account.");
                    return;
                }
                _repository.DeleteUser(userId);
                _view.ShowMessage("Employee deleted successfully.");
                LoadAndFilter("", "All");
            }
            catch (Exception ex)
            {
                _view.ShowError("Error deleting employee: " + ex.Message);
            }
        }
    }
}
