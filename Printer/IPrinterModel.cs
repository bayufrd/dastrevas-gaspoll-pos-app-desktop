using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Printer
{
        public interface IPrinterModel
        {
            Task SavePrinterSettings(string printerName);
            string LoadPrinterSettings();
        }
}
