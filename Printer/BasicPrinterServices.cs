using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Printer
{
    public class BasicPrinterService
    {
        // Metode Dasar Printing
        public void PrintDocument(PrintJob job)
        {
            try
            {
                // Validasi Input Sederhana
                ValidatePrintJob(job);

                // Konversi Konten
                byte[] printableContent = PreparePrintContent(job.DocumentContent);

                // Kirim ke Printer
                SendToPrinter(job.PrinterName, printableContent);

                // Log Sukses
                Console.WriteLine($"Berhasil mencetak dokumen {job.DocumentId}");
            }
            catch (Exception ex)
            {
                // Tangani Error
                HandlePrintError(ex);
            }
        }

        // Validasi Print Job
        private void ValidatePrintJob(PrintJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job), "Print job tidak boleh kosong");

            if (string.IsNullOrWhiteSpace(job.PrinterName))
                throw new ArgumentException("Nama printer harus ditentukan");

            if (job.DocumentContent == null || job.DocumentContent.Length == 0)
                throw new ArgumentException("Konten dokumen tidak boleh kosong");
        }

        // Persiapan Konten Cetak
        private byte[] PreparePrintContent(byte[] originalContent)
        {
            // Normalisasi Encoding
            return Encoding.UTF8.GetBytes(
                Encoding.UTF8.GetString(originalContent)
            );
        }

        // Kirim ke Printer
        private void SendToPrinter(string printerName, byte[] content)
        {
            try
            {
                // Simulasi pengiriman ke printer
                using (var printDocument = new System.Drawing.Printing.PrintDocument())
                {
                    printDocument.PrinterSettings.PrinterName = printerName;

                    // Event handler untuk printing
                    printDocument.PrintPage += (sender, e) =>
                    {
                        // Logika cetak aktual
                        using (var stream = new MemoryStream(content))
                        {
                            var image = System.Drawing.Image.FromStream(stream);
                            e.Graphics.DrawImage(image, 0, 0);
                        }
                    };

                    // Mulai cetak
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Gagal mengirim ke printer {printerName}", ex);
            }
        }

        // Penanganan Error Printing
        private void HandlePrintError(Exception ex)
        {
            // Log error
            Console.Error.WriteLine($"Error Printing: {ex.Message}");

            // Kategorisasi Error
            switch (ex)
            {
                case ArgumentException argEx:
                    Console.WriteLine("Masalah dengan input print job");
                    break;
                case System.ComponentModel.Win32Exception win32Ex:
                    Console.WriteLine("Masalah koneksi printer");
                    break;
                default:
                    Console.WriteLine("Error tidak dikenal");
                    break;
            }
        }
    }
    public class AdvancedPrinterManager
    {
        // Queue untuk manajemen print job
        private ConcurrentQueue<PrintJob> _printQueue = new ConcurrentQueue<PrintJob>();

        // Semaphore untuk kontrol konkurensi
        private SemaphoreSlim _printSemaphore = new SemaphoreSlim(1, 1);

        // Metode Enqueue Print Job
        public async Task EnqueuePrintJobAsync(PrintJob job)
        {
            // Tambahkan job ke antrian
            _printQueue.Enqueue(job);

            // Proses antrian
            await ProcessPrintQueueAsync();
        }

        // Proses Antrian Print
        private async Task ProcessPrintQueueAsync()
        {
            try
            {
                // Tunggu semaphore
                await _printSemaphore.WaitAsync();

                // Proses setiap job dalam antrian
                while (_printQueue.TryDequeue(out PrintJob job))
                {
                    try
                    {
                        // Cetak dengan timeout
                        await PrintWithTimeoutAsync(job, TimeSpan.FromSeconds(30));
                    }
                    catch (Exception jobEx)
                    {
                        // Log error per job
                        LogPrintJobError(job, jobEx);
                    }
                }
            }
            finally
            {
                // Selalu lepaskan semaphore
                _printSemaphore.Release();
            }
        }

        // Cetak dengan Timeout
        private async Task PrintWithTimeoutAsync(PrintJob job, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);

            try
            {
                await Task.Run(() =>
                {
                    // Logika cetak aktual
                    new BasicPrinterService().PrintDocument(job);
                }, cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Printing timed out");
            }
        }

        // Log Error Print Job
        private void LogPrintJobError(PrintJob job, Exception ex)
        {
            // Implementasi logging komprehensif
            Console.WriteLine($"Error mencetak job {job.DocumentId}: {ex.Message}");
        }
    }
    public class PrinterDiagnostics
    {
        // Cek status printer
        public static PrinterStatus CheckPrinterStatus(string printerName)
        {
            try
            {
                using (var printDocument = new System.Drawing.Printing.PrintDocument())
                {
                    printDocument.PrinterSettings.PrinterName = printerName;

                    return new PrinterStatus
                    {
                        IsAvailable = printDocument.PrinterSettings.IsValid,
                        PrinterName = printerName,
                        QueueCount = printDocument.PrinterSettings.MaximumCopies,
                        IsOnline = printDocument.PrinterSettings.IsValid
                    };
                }
            }
            catch (Exception ex)
            {
                return new PrinterStatus
                {
                    IsAvailable = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        // Struktur Status Printer
        public class PrinterStatus
        {
            public bool IsAvailable { get; set; }
            public string PrinterName { get; set; }
            public int QueueCount { get; set; }
            public bool IsOnline { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
