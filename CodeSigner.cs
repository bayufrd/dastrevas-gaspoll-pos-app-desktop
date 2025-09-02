using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;
using KASIR.Helper;

namespace KASIR
{
    public class CodeSigner
    {
        public static bool SignExecutable(string executablePath, string certificatePath, string password)
        {
            // Tambahkan validasi awal
            if (string.IsNullOrEmpty(executablePath))
            {
                NotifyHelper.Error("Path executable tidak valid");
                return false;
            }

            if (!File.Exists(executablePath))
            {
                NotifyHelper.Error($"File executable tidak ditemukan: {executablePath}");
                return false;
            }

            if (!File.Exists(certificatePath))
            {
                NotifyHelper.Error($"File sertifikat tidak ditemukan: {certificatePath}");
                return false;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = GetSignToolPath(), // Fungsi dinamis mencari SignTool
                    Arguments = $"sign /f \"{certificatePath}\" /p \"{password}\" /tr http://timestamp.digicert.com /fd SHA256 \"{executablePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true, // Tambahkan error output
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    // Log detail proses
                    //LoggerUtil.LogWarning($"SignTool Output: {output}");

                    if (!string.IsNullOrEmpty(error))
                    {
                        NotifyHelper.Error($"SignTool Error: {error}");
                        //LoggerUtil.LogError(new Exception(error), "Signing Error Details");
                        return false;
                    }

                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                //LoggerUtil.LogError(ex, "Signing Process Exception", ex);
                return false;
            }
        }

        // Fungsi pencarian dinamis SignTool
        private static string GetSignToolPath()
        {
            string[] possiblePaths = new[]
            {
        @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x64\signtool.exe",
        @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe",
        @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x64\signtool.exe"
    };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                    return path;
            }

            throw new FileNotFoundException("SignTool tidak ditemukan. Pastikan Windows SDK terinstal.");
        }
    }
}
