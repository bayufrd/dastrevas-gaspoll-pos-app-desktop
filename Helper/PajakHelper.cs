    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace KASIR.Helper
    {
        public static class PajakHelper
        {
            private static readonly string PajakFilePath = Path.Combine("setting", "PajakTemp.data");

            /// <summary>
            /// Mencoba membaca pajak dari file setting/PajakTemp.data
            /// </summary>
            /// <param name="pajakValue">Isi pajak jika file ada dan berhasil dibaca</param>
            /// <returns>True jika file ada dan berhasil dibaca, false jika tidak ada / error</returns>
            public static bool TryGetPajak(out string pajakValue)
            {
                pajakValue = string.Empty;

                try
                {
                    if (!File.Exists(PajakFilePath))
                        return false;

                    string content = File.ReadAllText(PajakFilePath).Trim();

                    if (string.IsNullOrEmpty(content))
                        return false;

                    pajakValue = content;
                    return true;
                }
                catch
                {
                    // kalau gagal baca (misalnya corrupt) dianggap false
                    return false;
                }
            }
        }
    }