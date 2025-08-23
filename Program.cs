using System.Diagnostics;
using System.Security.Principal;
using KASIR.Helper;
using KASIR.Network;
using Microsoft.Extensions.Configuration;

namespace KASIR
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Validasi Administrator
            if (!IsRunAsAdministrator())
            {
                // Restart aplikasi dengan hak administrator
                RestartAsAdministrator();
                return;
            }

            // Konfigurasi Keamanan Dasar
            ConfigureApplicationSecurity();

            // Inisialisasi Aplikasi
            InitializeApplication();

            // Jalankan Aplikasi Utama
            Application.Run(new Form1());
        }

        // Fungsi Validasi Hak Administrator
        private static bool IsRunAsAdministrator()
        {
            try
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(id);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        // Restart Aplikasi dengan Hak Administrator
        private static void RestartAsAdministrator()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Application.ExecutablePath,
                Verb = "runas" // Meminta hak administrator
            };

            try
            {
                Process.Start(startInfo);
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Gagal memulai aplikasi sebagai administrator: {ex.Message}",
                    "Kesalahan Keamanan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // Konfigurasi Keamanan Aplikasi
        private static void ConfigureApplicationSecurity()
        {
            // Nonaktifkan Visual Styles yang tidak aman
            Application.EnableVisualStyles();

            // Inisialisasi Konfigurasi Aplikasi
            ApplicationConfiguration.Initialize();

            // Nonaktifkan rendering teks default yang berpotensi tidak aman
            Application.SetCompatibleTextRenderingDefault(false);

            // Batasi jumlah window handle
            System.Environment.SetEnvironmentVariable("COMPlus_SvchostCount", "256");

            // Nonaktifkan debugging di production
#if !DEBUG
            DisableDebugging();
#endif
        }

        // Nonaktifkan Debugging
        private static void DisableDebugging()
        {
            try
            {
                // Nonaktifkan attach debugger
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            catch { }
        }

        // Inisialisasi Aplikasi
        private static void InitializeApplication()
        {
            LoggerUtil.LogWarning("Aplikasi Dimulai");
            ValidateApplicationIntegrity();
        }


        // Validasi Integritas Aplikasi
        private static void ValidateApplicationIntegrity()
        {
            try
            {
                // Dapatkan path executable
                string execPath = Application.ExecutablePath;

                // Validasi ukuran file
                FileInfo fileInfo = new FileInfo(execPath);
                if (fileInfo.Length > 50 * 1024 * 1024) // Maks 50 MB
                {
                    throw new Exception("Ukuran executable tidak valid");
                }

                // Tambahkan validasi hash atau signature di sini
            }
            catch (Exception ex)
            {
                NotifyHelper.Error(
                    $"Validasi integritas gagal: {ex.Message}"
                ); 
                LoggerUtil.LogError(ex,
                    $"Validasi integritas gagal: {ex.Message}",
                    "Kesalahan Keamanan",ex.Message
                );
            }
        }
    }
}