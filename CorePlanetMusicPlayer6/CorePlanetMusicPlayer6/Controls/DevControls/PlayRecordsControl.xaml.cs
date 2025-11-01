using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls.DevControls
{
    public sealed partial class PlayRecordsControl : UserControl
    {
        public PlayRecordsControl()
        {
            this.InitializeComponent();
        }

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            await PlayRecordHelper.TestAsync();
        }

        private void QueryDateDataButton_Click(object sender, RoutedEventArgs e)
        {
            _ = QueryDateDataAsync();
        }

        async Task QueryDateDataAsync()
        {
            List<PlayRecord> playRecords = await PlayRecordHelper.GetByYearMonthDayAsync(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            SetListView(playRecords);
        }

        void SetListView(List<PlayRecord> playRecords)
        {
            PlayRecordsListView.ItemsSource = null;
            PlayRecordsListView.ItemsSource = playRecords;
        }
    }
}
