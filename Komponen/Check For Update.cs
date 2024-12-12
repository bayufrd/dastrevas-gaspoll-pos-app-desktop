using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KASIR.Model;
using KASIR.Network;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO.Compression;
using System.Web;
using System.Runtime;
using System.Diagnostics.Tracing;
using System.Security.Policy;
using System.Runtime.CompilerServices;
namespace KASIR.Komponen
{
    public partial class Check_For_Update : Form
    {

        private readonly string currentVersion1;
        private BackgroundWorker backgroundWorker;
        public Check_For_Update()
        {
            //MessageBox.Show("INI VERSI TERBARU LOH");
            this.ControlBox = false;
            InitializeComponent();
            labelnew.Hide();
            lbnew.Hide();
            btnUpdate.Enabled = false;
            //bw_updatechecker.RunWorkerAsync();
            currentVersion1 = Properties.Settings.Default.Version.ToString();
            lbcurrent.Text = currentVersion1;

            

            //MessageBox.Show($"{currentVersion1}");
            btnUpdate.Click += async (sender, e) => await btnUpdate_Click(sender, e);
        }

        private void checkUpdate()
        {
            var urlVersion = "http://localhost/update/version.txt";
            var newVersion = (new WebClient().DownloadString(urlVersion));
            string currentVersion = currentVersion1;
            //var cekversi = Application.ProductVersion.ToString();
            //MessageBox.Show($"Your version is {cekversi}");
            newVersion = newVersion.Replace(".", "");
            currentVersion = currentVersion.Replace(".", "");
            this.Invoke(new Action(() =>
            {

                if (Convert.ToInt32(newVersion) > Convert.ToInt32(currentVersion))
                {
                    lbheader.Text = "A New Version is Available.\r\nDo you want to Update ?\r\n";
                    lbnew.Text = (new WebClient().DownloadString(urlVersion));
                    btnUpdate.Enabled = true;
                    lbnew.Show();
                    labelnew.Show();
                }
                else
                {
                    lbheader.Text = "The version is up to date";
                    btnUpdate.Enabled = false;
                    lbnew.Hide();
                    labelnew.Hide();
                }
            }));
        }
        static async Task DownloadFileAsync(string fileUrl, string destinationPath)
        {

            using (WebClient webClient = new WebClient())
            {

                /*
                // Download the file asynchronously
                byte[] fileBytes = await webClient.DownloadDataTaskAsync(new Uri(fileUrl));

                // Get the file name from the URL
                string fileName = Path.GetFileName(fileUrl);

                // Combine the download directory with the file name
                string filePath = Path.Combine(destinationPath, fileName);

                // Save the file to the specified location
                File.WriteAllBytes(filePath, fileBytes);

                MessageBox.Show($"File downloaded and saved to: {filePath}");
                */


                try
                {
                    byte[] fileData = webClient.DownloadData(fileUrl);
                    MessageBox.Show($"Downloaded {fileData.Length} bytes.");
                    System.IO.File.WriteAllBytes(destinationPath, fileData);
                    MessageBox.Show("Download complete.");

                    //string zipPath = "D:\\UpdateKasir\\update.zip";
                    //string extractPath = "D:\\UpdateKasir"; 
                    //ZipFile.ExtractToDirectory(zipPath, extractPath);
                    MessageBox.Show("Mohon tunggu sampai App restart");
                    
                    string msiPath = "D:\\UpdateKasir\\update.msi";
                    string installationLogPath = "D:\\UpdateKasir\\installation.log";
                    //string instalDir = "C:\\Program Files (x86)\\DTCoding\\KASIR";
                    //INSTALDIRR= mengatur install path msi
                    MessageBox.Show("Sedang menjalankan installer mohon bersabar, App saat ini mungkin tidak berjalan normal.");

                    try
                    {
                        Process process = new Process();
                        process.StartInfo.FileName = "msiexec.exe";
                        process.StartInfo.Arguments = $"/i \"{msiPath}\" /qn /l*v \"{installationLogPath}\" ";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.CreateNoWindow = true;

                        process.Start();

                        string output = process.StandardOutput.ReadToEnd();
                        //Console.WriteLine(output);
                        Application.Exit();

                        process.WaitForExit();

                        //Console.WriteLine("Installation completed successfully.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Instal Gagal");

                        //Console.WriteLine($"Error: {ex.Message}");
                    }
                    MessageBox.Show("Menutup Program...");
                    initScript();
                    /*
                    //declar
                    string msiPath = "D:\\UpdateKasir\\update.msi";
                    string arguments = "/QN";

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "msiexec.exe",
                        Arguments = $"/i {msiPath} {arguments}", // Specify the MSI path and additional arguments
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    using (Process process = new Process { StartInfo = startInfo })
                    {
                        process.Start();
                        //initScript();
                        // Asynchronously wait for the process to exit
                        TaskCompletionSource<object> processExited = new TaskCompletionSource<object>();
                        process.EnableRaisingEvents = true;
                        process.Exited += (sender, e) => processExited.SetResult(null);

                        // Wait for the process to exit or a timeout (adjust the timeout as needed)
                        Task timeoutTask = Task.Delay(TimeSpan.FromMinutes(5)); // Example: Timeout after 5 minutes
                        Task completedTask = await Task.WhenAny(processExited.Task, timeoutTask);

                        if (completedTask == timeoutTask)
                        {
                            MessageBox.Show("MSI installation timed out.");
                            // Handle timeout as needed
                        }
                        else
                        {
                            // Process exited within the timeout
                            MessageBox.Show("MSI installation completed.\r\nSilahkan membuka Aplikasi Kasir ulang.");
                            // Add additional logic as needed after the installation completes
                        }
                    }
                    */
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error downloading file: {ex.Message}");
                }
            }


        }

       
        static async Task btnUpdate_Click(object sender, EventArgs e)
        {
            
            /*
            using (DownloadProgress downloadProgress = new DownloadProgress())
            {
                DialogResult result = downloadProgress.ShowDialog();

                // Handle the result if needed
                if (result == DialogResult.OK)
                {
                    // Settings were successfully updated, perform any necessary actions
                }
            }
            */

            if (File.Exists("D:\\UpdateKasir\\update.msi")) { File.Delete("D:\\UpdateKasir\\update.msi");}
            string fileUrl = "http://localhost/update/update.msi";
            string destinationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "D:\\UpdateKasir\\update.msi");
            await DownloadFileAsync(fileUrl, destinationPath);

            
            await initScript();
            
            /*
              
             WebClient web = new WebClient();

            web.DownloadFileAsync(new Uri("http://localhost/update/update.zip"), "C:/Program Files/DTCoding/KASIR/update.zip");
            web.DownloadFileCompleted += Web_DownloadFileCompleted;
            */

            //startDownload();

            //string zipPath = "C:/Program Files/DTCoding/KASIR";
            //string extractPath = @".\";
            //ZipFile.ExtractToDirectory(zipPath, extractPath);
            //initScript();
            //Process process = new Process();
            //process.StartInfo.FileName = "msiexec";
            //process.StartInfo.Arguments = String.Format("/i update.msi");

            //this.Close();
            //process.Start();

        }

        static async Task startDownload()
        {
            //string fileUrl = "http://localhost/update/update.zip";
            string fileUrl = "https://doc-10-20-docs.googleusercontent.com/docs/securesc/ftulvkleqljgqlltki5gnetb88do3jpf/cpf5rl3oadbj5an93inq26g1kcalhq02/1702352400000/03547051722451280684/03547051722451280684/1ep_skGKQE3grJ1FzoRCIzqzIG-kuJWwW?e=download&ax=AEqgLxmcyp6b_ong9WQgYAyJExdkM64YfS-MQOFSad3LCLxqT7SFPoeyp6Z5vP0au16rFVBBVuecLMpZCDmNI_IYqNFl01IyL438ba2EsO6rdz40kNpFQr1E983VUHMlNMSkO9T81IkG7ydWXmMLpC-qAsMedQXko4TLY-s3vcb1ERi2dN8BzTZA2V00HwFgs5eVzuy1UoD9BaGA8D37g1B5Td80m9T027GkgI9dq2kVYoqzdCXqTPsDs00jd9YUpa3TDa8p8EXBJZhHN7Bmrvidx4jn0_BJvXehVprjkPU7w2X4Zn-O1uFqcnPKD60KyBXTMO3LuNHJRfrThisc7EPAPRwmNwkFIJJzqGNQu7Ec0J_Eb6qkvN2cF-JU1YqQ7TzyGdXH3VADJZ5O0HRQ_SmZlDfTeu7p8WYcWs6R3h0Ut_Ir9XBFRsKpTnQWADgyXTvN3wTp8a7YF_BVa7IQ7d86nIrw086uV32N5h6WhPk6-eKylaXCZBhgX2LMi7DdbOcw-0piT8YnoCAehyVqY9XQUpK_R8smRMMjSGAyDi74Xxo7d5AwQAy_nS41ZrHwOUmXTIh_ZXZKBt4o2tjCwWgTFzn4bk2XwMtvS4RiWTiDKjGt7m1O5wgp5PZmldLQj7yJOEZre06YLApqfyOVr0wnl8N8O7E7c-n-OHZNGNRhR6MRq8l501s2TFF9kyqwM0iHoMamK4ZvutY0wel16Jw3CLaSXifvCASUmiFuYG9Y6MZhslb5tCCbuwv2j5oEBnQ1LeqVFZefpWQY0Aghjie-laIT9G4SCOJUHXkeMuOX2a_wjO08UY0gy6JHPG0O1axAD7D96Vzm_ZoSQNh9KCw2jpcbnXjS92gif-6a4y1f9B5kjQ&uuid=96fc9c8f-fd56-4610-885c-ab5a723ff653&authuser=0&nonce=og891a3144bjk&user=03547051722451280684&hash=4cn79ttiohhucjs58jgmks3aj73hia43";
            string destinationPath = "D:\\DTCoding\\KASIR\\update.zip";

            //await DownloadFileAsync(fileUrl, destinationPath);
        }
        private void DownloadProgress()
        {
        }
        static async Task _DownloadFileAsync(string fileUrl, string destinationPath)
        {
            using (WebClient webClient = new WebClient())
            {
                try { // Register the event handler to track download progress
                    webClient.DownloadProgressChanged += (sender, e) =>
                    {
                        //long fileSize = GetFileSize(fileUrl);
                        //byte[] fileData = BitConverter.GetBytes(fileSize);
                        //did you receive the data successfully? Place your own condition here. 
                        //using (FileStream fileStream = new FileStream("C:\\Users\\Alex\\Desktop\\Data.zip", FileMode.Create))
                           //fileStream.Write(fileData, 0, fileData.Length);
                       
                       //dd2.Text = e.TotalBytesToReceive;
                       //MessageBox.Show($"Downloaded {e.BytesReceived} of {e.TotalBytesToReceive} bytes");
                   };

                    // Register the event handler to be notified when the download completes
                    TaskCompletionSource<bool> downloadCompleted = new TaskCompletionSource<bool>();
                    webClient.DownloadFileCompleted += (sender, e) =>
                    {
                        downloadCompleted.SetResult(true);
                    };

                    MessageBox.Show($"Downloading file from {fileUrl} to {destinationPath}...");

                    // Start the asynchronous download
                    webClient.DownloadFileAsync(new Uri(fileUrl), destinationPath);

                    // Wait for the download to complete
                    await downloadCompleted.Task;

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error closing: {ex.Message}");
                }
                }
        }

        private void Web_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            initScript();
        }

        static async Task initScript()
        {
            //MessageBox.Show("Download Selesai");
            string processName = "KASIR.exe";
            string path = Application.StartupPath + @"\bat.bat";

            Process p = new Process();
            p.StartInfo.FileName = path;
            p.StartInfo.Arguments = "";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.Verb = "runas";
            p.Start();
            Application.Exit();
            // Call the CloseApplication method to close the application
        }
        static void CloseApplication(string processName)
        {
            // Get all running processes with the specified process name
            Process[] processes = Process.GetProcessesByName(processName);

            // Iterate through the processes and close each one
            foreach (Process process in processes)
            {
                try
                {
                    // Close the application
                    process.CloseMainWindow();

                    // Wait for the application to close (optional)
                    process.WaitForExit();

                    // If the application did not close, kill the process
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }

                    Console.WriteLine($"Closed {processName} successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error closing {processName}: {ex.Message}");
                }
            }
        }
        private void BtnKeluar_Click(object sender, EventArgs e)
        {
            // Close the form with DialogResult.Cancel
            DialogResult = DialogResult.Cancel;
            Close();
        }
        private void bw_updatechecker_DoWork(object sender, DoWorkEventArgs e)
        {
            checkUpdate();
        }

        private void bw_updatechecker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bw_updatechecker.RunWorkerAsync();
        }
        
    }
}
    
    
