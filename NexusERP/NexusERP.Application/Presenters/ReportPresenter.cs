using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Application.Interfaces.Views;
using NexusERP.Domain.Entities;

namespace NexusERP.Application.Presenters
{
    public class ReportPresenter
    {
        private readonly IReportRepository _repository;
        private readonly IExcelExportService _excelService;
        private readonly IPdfExportService _pdfService;
        private IReportView _view = null!;

        public ReportPresenter(IReportRepository repository, IExcelExportService excelService, IPdfExportService pdfService)
        {
            _repository = repository;
            _excelService = excelService;
            _pdfService = pdfService;
        }

        public void SetView(IReportView view)
        {
            _view = view;
        }

        public void RefreshData()
        {
            try
            {
                IEnumerable<InventoryTransaction> dt = _repository.GetAll();
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
                IEnumerable<InventoryTransaction> dt = _repository.Search(Keyword);
                _view.GridDataSource = dt;
            }
            catch(Exception ex)
            {
                _view.ShowMessage(ex.Message);
            }
        }

        public void ExportExcel(string filePath, IEnumerable<InventoryTransaction> data)
        {
            try
            {
                _excelService.ExcelTransactions(data, filePath);
                _view.ShowMessage("Export succesful! File saved to: " + filePath);
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message);
            }
        }

        public void ExportPdf(string filePath, InventoryTransaction transaction)
        {
            try
            {
                _pdfService.GenerateInvoice(transaction, filePath);
                _view.ShowMessage("PDF Invoice saved successfully");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                _view.ShowMessage("Error generating PDF: " + ex.Message);
            }
        }
    }
}
