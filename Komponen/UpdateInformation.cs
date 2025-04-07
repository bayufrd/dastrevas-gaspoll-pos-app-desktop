namespace KASIR.Komponen
{
    public partial class UpdateInformation : Form
    {
        public UpdateInformation()
        {
            InitializeComponent();
            LoadInfoAsync();
        }

        private async void LoadInfoAsync()
        {
            try
            {
                string fileUrl = "https://raw.githubusercontent.com/bayufrd/update/main/update.txt";
                using (var httpClient = new HttpClient())
                {
                    string info = await httpClient.GetStringAsync(fileUrl);
                    // Create and configure the label on the UI thread
                    CreateInfoLabel(info);
                }
            }
            catch (Exception ex)
            {
                // Optional: Log the error or show a user-friendly message
                LoggerUtil.LogError(ex, "Error fetching info: {ErrorMessage}", ex.Message);
            }
        }

        private void CreateInfoLabel(string info)
        {
            // Ensure UI updates happen on the UI thread
            if (InvokeRequired)
            {
                Invoke(new Action(() => CreateInfoLabel(info)));
                return;
            }

            var strukLabel = new Label
            {
                Text = info,
                BackColor = Color.Transparent,
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.TopLeft,
                AutoSize = true,
                Font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold)
            };

            gradientPanel2.AutoScroll = true;
            gradientPanel2.Controls.Add(strukLabel);
        }
    }
}