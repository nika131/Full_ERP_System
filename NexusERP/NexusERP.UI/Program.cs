using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Application.Presenters;
using NexusERP.Domain.Enums;
using NexusERP.Infrastructure.Repositories;
using NexusERP.Infrastructure.Services;
using Product_Inventory_Manager.Product_Inventory_Manager.Views;
using QuestPDF.Infrastructure;
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
            QuestPDF.Settings.License = LicenseType.Evaluation;

            ApplicationConfiguration.Initialize();

            var services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();

            var LoginForm = serviceProvider.GetRequiredService<LoginForm>();
            System.Windows.Forms.Application.Run(LoginForm);

        }

        

        private static void ConfigureServices(ServiceCollection services)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var jwtSecret = config["JwtSettings:Secret"];

            services.AddSingleton<IConfiguration>(config);


            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<IReportRepository, ReportRepository>();
            services.AddTransient<ISupplierRepository, SupplierRepository>();

            services.AddTransient<IExcelExportService, ExcelExportService>();
            services.AddTransient<IPdfExportService, PdfExportService>();

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IAuthService, AuthService>();

            services.AddTransient<ProductPresenter>();
            services.AddTransient<ReportPresenter>();
            services.AddTransient<SupplierPresenter>();
            services.AddTransient<DashboardPresenter>();
            services.AddTransient<LoginPresenter>();
            services.AddTransient<RegisterPresenter>();

            services.AddTransient<DashboardForm>();
            services.AddTransient<ReportsForm>();
            services.AddTransient<SupplierForm>();
            services.AddTransient<ProductForm>();
            services.AddTransient<MainShellForm>();
            services.AddTransient<LoginForm>();
            services.AddTransient<RegisterForm>();

            
        }
    }
}