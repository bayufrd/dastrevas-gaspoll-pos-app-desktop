using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO.Pipes;

namespace KASIR.DualScreen
{
    public class SignalSender
    {
        private const string PipeName = "signalPipe"; // Nama Named Pipe

        public void SendSignal(string signal)
        {
            try
            {
                using (var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    pipeClient.Connect();
                    byte[] messageBytes = Encoding.UTF8.GetBytes(signal);
                    pipeClient.Write(messageBytes, 0, messageBytes.Length);
                    //Console.WriteLine($"Sent signal: {signal}");
                }
            }catch( Exception ex)
            {
                LoggerUtil.LogError(ex, "An error occurred: {ErrorMessage}", ex.Message);
            }
        }
    }
}
