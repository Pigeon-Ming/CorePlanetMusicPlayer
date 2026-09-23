using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer6.Composition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace CorePlanetMusicPlayer6.Controls.Dev
{
    public sealed partial class LibraryControl : UserControl
    {
        private bool _isBusy;

        public LibraryControl()
        {
            this.InitializeComponent();
        }

        public Task ReloadAsync()
        {
            return RunOperationAsync(services => Task.FromResult("来源列表已重新读取。"));
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;

            LibraryCommandBar.IsEnabled = !isBusy;
            LibraryFoldersListView.IsEnabled = !isBusy;

            UpdateRemoveButton();
        }

        private void UpdateRemoveButton()
        {
            RemoveFolderButton.IsEnabled = !_isBusy && LibraryFoldersListView.SelectedItem is LibraryFolder;
        }

        private async Task<string> AddFolderAsync(AppServices services)
        {
            var folder = await services.StorageAccessService
                .PickerAndCreateFutureAccessFolderAsync();

            if (folder == null)
            {
                return "已取消添加。";
            }

            try
            {
                await services.MusicLibraryService.AddFolderAsync(folder);
            }
            catch
            {
                // 选择文件夹时已经登记了访问授权。
                // 保存失败时，尝试清理本次新增的授权。
                try
                {
                    services.StorageAccessService.RemoveFutureAccess(folder);
                }
                catch (Exception cleanupException)
                {
                    Debug.WriteLine(cleanupException);
                }

                throw;
            }

            return $"已添加：{folder.DisplayName}";
        }

        private async Task<string> RemoveFolderAsync(AppServices services, LibraryFolder folder)
        {
            await services.MusicLibraryService.RemoveFolderAsync(folder.Id);

            if (folder.CanRestoreByAccessKey)
            {
                services.StorageAccessService.RemoveFutureAccess(folder);
            }

            return $"已移除：{folder.DisplayName}";
        }

        private void LibraryFoldersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRemoveButton();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadAsync();
        }
        
        private async void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            await RunOperationAsync(AddFolderAsync);
        }

        private async void RemoveFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folder = LibraryFoldersListView.SelectedItem as LibraryFolder;

            if (folder == null)
            {
                return;
            }

            await RunOperationAsync(services => RemoveFolderAsync(services, folder));
        }

        private async Task RunOperationAsync(Func<AppServices, Task<string>> operation)
        {
            if (_isBusy)
            {
                return;
            }

            var services = AppRuntime.Services;

            if (services == null ||
                services.MusicLibraryService == null ||
                services.StorageAccessService == null)
            {
                StatusTextBlock.Text = "服务尚未就绪，请检查应用初始化。";
                return;
            }

            SetBusy(true);
            StatusTextBlock.Text = "正在执行……";

            try
            {
                string message;

                try
                {
                    message = await operation(services);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    message = $"操作未完整完成：{ex.Message}";
                }

                // 即使操作失败，也重新读取实际保存的数据。
                // 当前移除流程包含多个步骤，失败前可能已有部分改动。
                try
                {
                    var folders = await services.MusicLibraryService
                        .GetAllFoldersAsync();

                    LibraryFoldersListView.ItemsSource = folders;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    message += $"\n重新读取来源失败：{ex.Message}";
                }

                StatusTextBlock.Text = message;
            }
            finally
            {
                SetBusy(false);
            }
        }
    }
}
