using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Printer
{
    public class PrinterItem
    {
        public string PrinterName { get; set; }
        public string PrinterId { get; set; }

        public PrinterItem(string printerName, string printerId)
        {
            PrinterName = printerName;
            PrinterId = printerId;
        }

        public override string ToString()
        {
            return PrinterName;
        }
    }
}
