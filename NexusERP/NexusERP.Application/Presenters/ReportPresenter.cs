using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Application.Interfaces.Views;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.State;

namespace NexusERP.Application.Presenters
{
    public class ReportPresenter
    {
        private readonly IReportRepository _repository;
        private readonly IExcelExportService _excelService;
        private readonly IPdfExportService _pdfService;
        private IReportView _view = null!;

        private IEnumerable<InventoryTransaction> _allLoadedTransactions = new List<InventoryTransaction>();

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
                _allLoadedTransactions = _repository.GetAll();
                var secureData = ApplyRbacRestrictions(_allLoadedTransactions);
                _view.GridDataSource = secureData;
            }
            catch (Exception ex)
            {
                _view.ShowMessage("Database Error: " + ex.Message);
            }
        }

        public void Search(string Keyword)
        {
            try
            {
                _allLoadedTransactions = _repository.Search(Keyword);
                var secureData = ApplyRbacRestrictions(_allLoadedTransactions);
                _view.GridDataSource = secureData;
            }
            catch(Exception ex)
            {
                _view.ShowMessage("Search Error: " +  ex.Message);
            }
        }

        public void FilterByType(string typeFilter)
        {
            var secureData = ApplyRbacRestrictions(_allLoadedTransactions);

            if (typeFilter == "All")
            {
                _view.GridDataSource = secureData;
            }
            else if (Enum.TryParse(typeFilter, out TransactionAction action))
            {
                _view.GridDataSource = secureData.Where(t => t.TransactionType == action).ToList();
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

        private IEnumerable<InventoryTransaction> ApplyRbacRestrictions(IEnumerable<InventoryTransaction> rawData)
        {
            if (UserSession.Role == Domain.Enums.UserRole.Cashier)
            {
                return rawData.Where(t => t.TransactionType == Domain.Enums.TransactionAction.Sale && t.UserId == UserSession.UserId).ToList();
            }

            return rawData;
        }
    }
}
