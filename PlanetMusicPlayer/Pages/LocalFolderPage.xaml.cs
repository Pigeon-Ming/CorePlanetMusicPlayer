using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib.Ape;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace PlanetMusicPlayer.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LocalFolderPage : Page
    {
        StorageFolder currentFolder {  get; set; }
        StorageFile currentFile { get; set; }

        public LocalFolderPage()
        {
            this.InitializeComponent();
            currentFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            enterFolderAsync(currentFolder);
            Debug.WriteLine(Windows.Storage.ApplicationData.Current.LocalFolder.Path);
        }

        public async Task enterFolderAsync(StorageFolder folder)
        {
            FolderPath_Text.Text = folder.Path;
            List<FileManagerDisplayItem> displayItems = new List<FileManagerDisplayItem>();

            IReadOnlyList<IStorageItem> itemsList = await folder.GetItemsAsync();
            foreach (var item in itemsList)
            {
                if(item is StorageFolder)
                {
                    displayItems.Add(new FileManagerDisplayItem { Name = ((StorageFolder)item).Name, folder = (StorageFolder)item ,type = "文件夹"});
                }
                else if(item is StorageFile)
                {
                    displayItems.Add(new FileManagerDisplayItem { Name = ((StorageFile)item).Name, file = (StorageFile)item, type = "文件" });
                }
            }
            MainListView.ItemsSource = displayItems;
        }

        private async void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            await Windows.Storage.FileIO.WriteTextAsync(currentFile, FileContentBox.Text);
        }

        private async void SaveFileAs_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            currentFile.DeleteAsync();
        }

        private async void MainListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            FileManagerDisplayItem item = ((FileManagerDisplayItem)e.ClickedItem);
            if (item.type == "文件")
            {
                Debug.WriteLine("文件：" + item.file.Path);
                FileContentBox.Text = await Windows.Storage.FileIO.ReadTextAsync(item.file); 
                currentFile = item.file;
            }else if(item.type == "文件夹")
            {
                
                currentFolder = item.folder;
                enterFolderAsync(currentFolder);
            }
        }

        private void BackToRoot_Click(object sender, RoutedEventArgs e)
        {
            currentFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            enterFolderAsync(currentFolder);
        }

        private void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            enterFolderAsync(currentFolder);
        }
    }
}
