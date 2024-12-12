using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KASIR.Komponen
{
    public partial class DownloadProgress : Form
    {
        public DownloadProgress()
        {
            InitializeComponent();

            //MessageBox.Show("KIMAK TEST");
            //if (File.Exists("D:\\UpdateKasir\\update.zip")) { File.Delete("D:\\UpdateKasir\\update.zip"); File.Delete("D:\\UpdateKasir\\update.msi"); }
            DownloadMulai();
            lbDownload.Text = "Instaling.......";

        }

        public void DownloadMulai()
        {
            string url = "http://localhost/update/update.zip";
            string destinationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "D:\\UpdateKasir\\update.zip");


            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += WebClientDownloadFileCompleted;
                webClient.DownloadProgressChanged += wc_DownloadProgressChanged;
                //webClient.DownloadDataAsync(new Uri(url), destinationPath); 

            }

        }
        private static void WebClientDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string url = "http://localhost/update/update.zip";
                string destinationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "D:\\UpdateKasir\\update.zip");

                // Access the downloaded data as a byte array
                WebClient webClient = (WebClient)sender;
                byte[] fileData = webClient.DownloadData(url);

                // Save the byte array to a file
                System.IO.File.WriteAllBytes(destinationPath, fileData);
                string zipPath = "D:\\UpdateKasir\\update.zip";
                string extractPath = "D:\\UpdateKasir";
                ZipFile.ExtractToDirectory(zipPath, extractPath);
                //initScript();
                MessageBox.Show("Download Completed!");

                string processName = "KASIR.exe";
                string path = Application.StartupPath + @"\bat.bat";
                //Console.WriteLine("Waiting for 5 seconds...");

                Process p = new Process();
                p.StartInfo.FileName = path;
                p.StartInfo.Arguments = "";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.Verb = "runas";
                p.Start();
            }
            else
            {
                MessageBox.Show($"Error: {e.Error.Message}");
            }
        }

        
       
        
        public void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            labelStatus.Text = "Processing... " + e.ProgressPercentage + "%";

            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            lbDownload.Text = "Downloaded: " + bytesIn + "KB/" + totalBytes + "KB";
        }
        public void initScript()
        {
            //MessageBox.Show("Download Selesai");
            string processName = "KASIR.exe";
            string path = Application.StartupPath + @"\bat.bat";
            //Console.WriteLine("Waiting for 5 seconds...");
            lbDownload.Text = "Instaling.......";

            Process p = new Process();
            p.StartInfo.FileName = path;
            p.StartInfo.Arguments = "";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.Verb = "runas";
            p.Start();
            
        }
       
    }
}
