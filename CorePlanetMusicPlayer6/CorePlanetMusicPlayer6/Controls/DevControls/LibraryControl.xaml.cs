using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.App;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
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
            RemovableDeviceManager.RemovableDevices.CollectionChanged += RemovableDevices_CollectionChanged;
        }

        private async void RemovableDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                refreshListView();
            });
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
            foreach (var item in RemovableDeviceManager.RemovableDevices)
            {
                list.Add(new ListItem { Token = "[可移动设备]", StorageFolder = item.StorageFolder});
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

        private async void TryOpenRemovableDeviceFileViaPathButton_Click(object sender, RoutedEventArgs e)
        {
            await TryOpenRemovableDeviceFileViaPathAsync();
        }

        async Task TryOpenRemovableDeviceFileViaPathAsync()
        {
            StorageFile storageFile = await StorageFile.GetFileFromPathAsync("F:\\杂项\\1.flac");
            BasicProperties basicProperties = await storageFile.GetBasicPropertiesAsync();
            Debug.WriteLine(storageFile.Name);
            Debug.WriteLine(basicProperties.Size);
        }
    }
}
