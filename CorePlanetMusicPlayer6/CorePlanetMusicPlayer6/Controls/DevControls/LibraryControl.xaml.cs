using CorePlanetMusicPlayer6.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class LibraryControl : UserControl
    {
        public LibraryControl()
        {
            this.InitializeComponent();
            refreshListView();
        }

        public class ListItem
        {
            public string Token { get; set; }

            public StorageFolder StorageFolder { get; set; }
        }

        void refreshListView()
        {
            List<ListItem> list = new List<ListItem>();
            foreach (var item in Library.Folders)
            {
                list.Add(new ListItem { Token = item.Value, StorageFolder = item.Key});
            }
            FolderListView.ItemsSource = null;
            FolderListView.ItemsSource = list;
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await refreshAsync();
        }

        async Task refreshAsync()
        {
            await Library.GetFoldersFromFutureAccessListAsync();
            refreshListView();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            await add();
        }

        async Task add()
        {
            await Library.AddFolderFromFolderPickerAsync();
            refreshListView();
        }

        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            await remove();
        }

        async Task remove()
        {
            if (FolderListView.SelectedItem == null)
                return;
            await Library.RemoveFolderAsync(((ListItem)FolderListView.SelectedItem).StorageFolder);
        }
    }
}
