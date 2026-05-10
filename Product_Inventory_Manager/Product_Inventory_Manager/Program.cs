using Product_Inventory_Manager.Product_Inventory_Manager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Product_Inventory_Manager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += (sender, e) =>
            {
                MessageBox.Show("A Critical Error Has Occured" + e.Exception.Message);
            };

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            Application.Run(new MainShell());
        }
    }
}
