using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.OffineMode
{
    public class CartSyncClient
    {
        public static void SendReloadSignal()
        {
            try
            {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "CartSyncPipe", PipeDirection.Out))
                {
                    pipeClient.Connect(1000); // Timeout 1 detik

                    using (StreamWriter writer = new StreamWriter(pipeClient))
                    {
                        writer.Write("RELOAD_CART");
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error atau handle silent
                LoggerUtil.LogError(ex, "Reload Signal Error: {ErrorMessage}", ex.Message);
            }
        }
    }
}
