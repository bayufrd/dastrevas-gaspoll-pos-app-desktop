using KASIR.Network;
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace KASIR
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Application.EnableVisualStyles();
            ApplicationConfiguration.Initialize();
            // Increase the maximum number of window handles that the application can create.
            Application.SetCompatibleTextRenderingDefault(false);
            System.Environment.SetEnvironmentVariable("COMPlus_SvchostCount", "256");
            // hapus dari atas jika masih crash
            Application.Run(new Form1());
        }

    }
}