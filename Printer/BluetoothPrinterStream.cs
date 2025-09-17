using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using InTheHand.Net.Sockets;

namespace KASIR.Printer
{
    public sealed class BluetoothPrinterStream : Stream
    {
        private readonly BluetoothClient _client;
        private readonly Stream _innerStream;

        public BluetoothPrinterStream(BluetoothClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _innerStream = client.GetStream();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _innerStream?.Dispose();
                _client?.Dispose();
            }
            base.Dispose(disposing);
        }

        // --- Implementasi wajib Stream ---
        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;
        public override long Position
        {
            get => _innerStream.Position;
            set => _innerStream.Position = value;
        }

        public override void Flush() => _innerStream.Flush();
        public override int Read(byte[] buffer, int offset, int count) =>
            _innerStream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) =>
            _innerStream.Seek(offset, origin);
        public override void SetLength(long value) =>
            _innerStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) =>
            _innerStream.Write(buffer, offset, count);
    }
}
