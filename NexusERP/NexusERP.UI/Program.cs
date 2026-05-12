using Microsoft.Extensions.DependencyInjection;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Application.Presenters;
using NexusERP.Infrastructure.Repositories;
using NexusERP.Infrastructure.Services;
using Product_Inventory_Manager.Product_Inventory_Manager.Views;
using System;
using System.Windows.Forms;

namespace NexusERP.UI
{
    internal static class Program
    {
        public static IServiceProvider serviceProvider { get; private set; } = null!;

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
            
            var mainForm = serviceProvider.GetRequiredService<MainShellForm>();
            System.Windows.Forms.Application.Run(mainForm);
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<IReportRepository, ReportRepository>();
            services.AddTransient<ISupplierRepository, SupplierRepository>();

            services.AddTransient<IExcelExportService, ExcelExportService>();
            services.AddTransient<IPdfExportService, PdfExportService>();

            services.AddTransient<ProductPresenter>();
            services.AddTransient<ReportPresenter>();
            services.AddTransient<SupplierPresenter>();
            services.AddTransient<DashboardPresenter>();

            services.AddTransient<DashboardForm>();
            services.AddTransient<ReportsForm>();
            services.AddTransient<SupplierForm>();
            services.AddTransient<ProductForm>();
            services.AddTransient<MainShellForm>();
        }
    }
}