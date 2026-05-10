using Product_Inventory_Manager.Product_Inventory_Manager.Repositories;
using Product_Inventory_Manager.Product_Inventory_Manager.Views.Interfaces;
using Product_Inventory_Manager.Product_Inventory_Manager.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product_Inventory_Manager.Product_Inventory_Manager.Presenters
{
    internal class ReportPresenter
    {
        private readonly IReportRepository _repository;
        private readonly IReportView _view;

        public ReportPresenter(IReportRepository repository, IReportView view)
        {
            _repository = repository; 
            _view = view;
        }

        public void RefreshData()
        {
            try
            {
                DataTable dt = _repository.GetAll();
                _view.GridDataSource = dt;
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message);
            }
        }

        public void Search(string Keyword)
        {
            try
            {
                DataTable dt = _repository.Search(Keyword);
                _view.GridDataSource = dt;
            }
            catch(Exception ex)
            {
                _view.ShowMessage(ex.Message);
            }
        }

        public void exportReport(string filePath, DataTable dt)
        {
            try
            {
                if (dt == null) throw new Exception("No data available to export.");

                ExportService export = new ExportService();
                export.ExportDataTableToExcel(dt, filePath, "Current Inventory");

                _view.ShowMessage("Export succesful! File saved to: " + filePath);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message);
            }
        }
    }
}
