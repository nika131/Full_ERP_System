using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Views;
using NexusERP.Domain.Entities;

namespace NexusERP.Application.Presenters
{
    public class SupplierPresenter
    {
        private ISupplierView _view = null!;
        private readonly ISupplierRepository _repository;

        public SupplierPresenter(ISupplierRepository repository)
        {
            _repository = repository;
        }

        public void SetView(ISupplierView view)
        {
            _view = view;
        }

        public void RefreshData()
        {
            try
            {
                IEnumerable<Supplier> suppliers = _repository.GetAllSuppliers();
                _view.SupplierGridDataSource = suppliers;
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message);
            }
        }

        public void SearchSuppliers(string keyword)
        {
            try
            {
                IEnumerable<Supplier> dt = _repository.SearchSuppliers(keyword);
                _view.SupplierGridDataSource = dt;
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message);
            }
        }
        public void SaveSupplier()
        {
            if (string.IsNullOrEmpty(_view.ViewCompanyName))
            {
                _view.ShowMessage("Company Name is Required");
                return;
            }
            try
            {
                _repository.UpsertSuppliers(new Supplier
                {
                    SupplierId = _view.SupplierId,
                    CompanyName = _view.ViewCompanyName,
                    ContactName = _view.ContactName,
                    Phone = _view.Phone,
                    Email = _view.Email
                });
                RefreshData();
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message);
            }
        }

        public void DeleteSupplier(int id)
        {
            try
            {
                _repository.DeleteSupplier(id);
                RefreshData();
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message);
            }
        }
    }
}
