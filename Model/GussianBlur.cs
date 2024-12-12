using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class GaussianBlur
    {
        public void Blur(Graphics graphics, Rectangle rectangle, int blurSize)
        {
            int width = rectangle.Width;
            int height = rectangle.Height;

            Bitmap bitmap = new Bitmap(width, height);
            bitmap.SetResolution(graphics.DpiX, graphics.DpiY);

            using (Graphics bitmapGraphics = Graphics.FromImage(bitmap))
            {
                bitmapGraphics.CopyFromScreen(rectangle.Location, Point.Empty, rectangle.Size);
            }

            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            int stride = bitmapData.Stride;
            IntPtr scan0 = bitmapData.Scan0;

            int halfBlurSize = blurSize / 2;

            byte[] buffer = new byte[stride * height];
            byte[] newBuffer = new byte[stride * height];

            Marshal.Copy(scan0, buffer, 0, buffer.Length);

            for (int y = 0; y < height; y++)
            {
                int sy = Math.Max(0, y - halfBlurSize);
                int ey = Math.Min(height, y + halfBlurSize + 1);

                for (int x = 0; x < width; x++)
                {
                    int sx = Math.Max(0, x - halfBlurSize);
                    int ex = Math.Min(width, x + halfBlurSize + 1);

                    float r = 0;
                    float g = 0;
                    float b = 0;

                    int count = 0;

                    for (int py = sy; py < ey; py++)
                    {
                        int location = py * stride + sx * bytesPerPixel;

                        for (int px = sx; px < ex; px++)
                        {
                            byte bb = buffer[location];
                            byte gg = buffer[location + 1];
                            byte rr = buffer[location + 2];

                            r += rr;
                            g += gg;
                            b += bb;

                            count++;

                            location += bytesPerPixel;
                        }
                    }

                    r /= count;
                    g /= count;
                    b /= count;

                    int index = y * stride + x * bytesPerPixel;

                    newBuffer[index] = (byte)r;
                    newBuffer[index + 1] = (byte)g;
                    newBuffer[index + 2] = (byte)b;
                }
            }

            Marshal.Copy(newBuffer, 0, scan0, newBuffer.Length);

            bitmap.UnlockBits(bitmapData);

            graphics.DrawImage(bitmap, rectangle);
        }

    }
}
