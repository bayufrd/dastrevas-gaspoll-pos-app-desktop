using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public static class PrinterConfiguration
    {
        // Regex untuk Macaddress dengan pemisah : atau -
        private static readonly Regex MacAddressRegex = new Regex(
            @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$",
            RegexOptions.Compiled);

        // Metode untuk memeriksa apakah string adalah IP address atau Macaddress
        public static bool IsValidIPAddress(string address)
        {
            return IPAddress.TryParse(address, out _) || IsValidMacAddress(address);
        }

        // Metode untuk memeriksa apakah string adalah Macaddress
        public static bool IsValidMacAddress(string address)
        {
            return MacAddressRegex.IsMatch(address);
        }
    }
}
