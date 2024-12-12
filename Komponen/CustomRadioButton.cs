using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace KASIR.Komponen
{
    public class CustomRadioButton : RadioButton
    {
        public CustomRadioButton()
        {
            // Remove the circle and adjust the appearance of the radio button
            FlatStyle = FlatStyle.Flat;
            Appearance = Appearance.Button;
            BackColor = SystemColors.Control;
            TextAlign = ContentAlignment.MiddleCenter;
            AutoSize = false;
            Size = new Size(150, 30); // Set the desired size for the radio button
        }

        protected override bool ShowFocusCues
        {
            get { return false; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw a custom rectangle for the radio button
            var radioSize = new Size(16, 16); // Adjust the size of the rectangle as needed
            var radioRect = new Rectangle(new Point(1, (Height - radioSize.Height) / 2), radioSize);
            ControlPaint.DrawRadioButton(e.Graphics, radioRect, Checked ? ButtonState.Checked : ButtonState.Normal);

            // Adjust the text position
            var textRect = new Rectangle(radioRect.Right + 4, 0, Width - radioRect.Right - 4, Height);

            // Draw the text
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, ForeColor, TextFormatFlags.VerticalCenter);
        }
    }
}
