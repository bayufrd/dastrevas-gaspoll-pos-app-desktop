using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KASIR.Printer.LogContext;

namespace KASIR.Printer
{
    public class LogContext
    {
        public enum PrintType
        {
            Payform,
            DataBill,
            CustomerReceipt,
            Checker,
            Kitchen,
            Bar,
            Refund,
            ShiftReport
        }

        public static void LogPrintOperation(
            PrintType printType,
            Exception ex = null,
            string additionalInfo = null)
        {
            string baseMessage = $"Print Operation: {printType}";

            if (!string.IsNullOrEmpty(additionalInfo))
            {
                baseMessage += $" - {additionalInfo}";
            }

            if (ex != null)
            {
                LoggerUtil.LogError(ex, baseMessage);
            }
            else
            {
                LoggerUtil.LogNetwork(baseMessage);
            }
        }
    }
    public enum PrinterErrorCode
    {
        ConnectionFailed,
        ConfigurationError,
        PrintingError,
        UnsupportedPrinterType
    }

    public class PrinterException : Exception
    {
        public PrinterErrorCode ErrorCode { get; }

        public PrinterException(
            string message,
            PrinterErrorCode errorCode,
            Exception innerException = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
    // Struktur Sederhana Print Job
    public class PrintJob
    {
        // Informasi Dasar Print Job
        public string DocumentId { get; set; }
        public string PrinterName { get; set; }
        public byte[] DocumentContent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
