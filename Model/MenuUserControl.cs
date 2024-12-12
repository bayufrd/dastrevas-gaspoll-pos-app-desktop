using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Model
{
    public class MenuUserControl : UserControl
    {
        private Label _nameLabel;
        private Label _typeLabel;
        private Label _priceLabel;
        private PictureBox _pictureBox;

        public MenuUserControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            _nameLabel = new Label
            {
                Location = new Point(10, 10),
                Text = "Name",
                AutoSize = true
            };
            Controls.Add(_nameLabel);

            _typeLabel = new Label
            {
                Location = new Point(10, 30),
                Text = "Type",
                AutoSize = true
            };
            Controls.Add(_typeLabel);

            _priceLabel = new Label
            {
                Location = new Point(10, 50),
                Text = "Price",
                AutoSize = true
            };
            Controls.Add(_priceLabel);

            _pictureBox = new PictureBox
            {
                Location = new Point(10, 70),
                Size = new Size(100, 100),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            Controls.Add(_pictureBox);

        }

        public Menu Menu
        {
            get;
            set;
        }

        private void LoadMenu()
        {
            if (Menu != null)
            {
                _nameLabel.Text = Menu.name;
                _typeLabel.Text = Menu.menu_type;
                _priceLabel.Text = $"{Menu.price}";
                LoadImageToPictureBox(_pictureBox, Menu);
            }
        }

        private async void LoadImageToPictureBox(PictureBox pictureBox, Menu menu)
        {
            string imagePath = GetImagePath(menu);
            if (File.Exists(imagePath))
            {
                pictureBox.Image = Image.FromFile(imagePath);
            }
            else
            {
                // Download the image and save it to the imagePath
                using (WebClient webClient = new WebClient())
                {
                    byte[] imageBytes = await webClient.DownloadDataTaskAsync(menu.image_url);
                    File.WriteAllBytes(imagePath, imageBytes);
                    pictureBox.Image = Image.FromFile(imagePath);
                }
            }
        }

        private string GetImagePath(Menu menu)
        {
            return Path.Combine(Application.StartupPath, "images", $"{menu.id}.jpg");
        }

    }
}
