using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace PlanetMusicPlayer.Models
{
    public class ContentDialogHelper
    {
        static ContentDialog contentDialog { get; set; } = new ContentDialog();

        public static async Task ShowContentDialogAsync(object Content)
        {
            if (contentDialog != null)
                contentDialog.Hide();
            contentDialog = new ContentDialog();
            contentDialog.Content = Content;
            contentDialog.VerticalContentAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
            contentDialog.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
            var result = await contentDialog.ShowAsync();
        }

        public static void HideDialog()
        {
            if (contentDialog != null)
                contentDialog.Hide();
        }
    }
}
