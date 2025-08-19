
using System;
using System.Windows.Forms;
using System.Reflection;

namespace KASIR.Helper
{
    public static class NotifyHelper
    {
        public static void ShowAlert(string message, NotifAlert.enmType type)
        {
            // Pastikan pemanggilan dilakukan di thread UI
            if (Application.OpenForms.Count > 0)
            {
                Application.OpenForms[0].BeginInvoke(new Action(() => {
                    try
                    {
                        ShowAlertInternal(message, type);
                    }
                    catch (Exception ex)
                    {
                        // Logging atau penanganan error tambahan jika diperlukan
                        System.Diagnostics.Debug.WriteLine($"Notify Error: {ex.Message}");
                    }
                }));
            }
            else
            {
                // Fallback jika tidak ada form terbuka
                ShowAlertInternal(message, type);
            }
        }

        private static void ShowAlertInternal(string message, NotifAlert.enmType type)
        {
            NotifAlert alert = new NotifAlert();
            var method = typeof(NotifAlert).GetMethod("showAlert",
                BindingFlags.NonPublic | BindingFlags.Instance);

            method.Invoke(alert, new object[] { message, type });
        }

        // Metode statik untuk kemudahan pemanggilan
        public static void Success(string message)
        {
            ShowAlert(message, NotifAlert.enmType.Success);
        }

        public static void Warning(string message)
        {
            ShowAlert(message, NotifAlert.enmType.Warning);
        }

        public static void Error(string message)
        {
            ShowAlert(message, NotifAlert.enmType.Error);
        }

        public static void Info(string message)
        {
            ShowAlert(message, NotifAlert.enmType.Info);
        }
    }
}
