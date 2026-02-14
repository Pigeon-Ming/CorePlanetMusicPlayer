using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib.Riff;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.App
{
    public class CoverLoaderConverter : IValueConverter
    {
        private static readonly BitmapImage DefaultCover = new BitmapImage(new Uri("ms-appx:///Assets/DefaultCover.png"));
        private static readonly ConcurrentDictionary<string, BitmapImage> Cache = new ConcurrentDictionary<string, BitmapImage>(StringComparer.OrdinalIgnoreCase);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var filePath = value as string;
            //Debug.WriteLine($"[CoverLoaderConverter] Convert called. valueType={(value == null ? "null" : value.GetType().FullName)}, filePath='{filePath}'");

            if (string.IsNullOrEmpty(filePath))
            {
                //Debug.WriteLine("[CoverLoaderConverter] Returning DefaultCover (empty path).");
                return DefaultCover;
            }

            if (Cache.TryGetValue(filePath, out var cached) && cached != null)
            {
                //Debug.WriteLine($"[CoverLoaderConverter] Returning cached BitmapImage for '{filePath}'.");
                return cached;
            }

            

            // 异步加载并在完成后更新 placeholder
            if (filePath.StartsWith("http") && filePath.Contains("://"))
            {
                //TODO: 优化网络封面的加载
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new Uri(filePath);
                return bitmapImage;
            }
            else
            {
                // 创建占位 BitmapImage 并立即返回（保证返回类型是 ImageSource）
                var placeholder = new BitmapImage();
                Cache[filePath] = placeholder;
                _ = LoadAndSetAsync(filePath, placeholder);
                return placeholder;
            }

            //Debug.WriteLine($"[CoverLoaderConverter] Returning placeholder for '{filePath}'.");
            
        }

        private async Task LoadAndSetAsync(string filePath, BitmapImage bitmap)
        {
            try
            {
                StorageFile file = null;
                try
                {
                    file = await StorageFile.GetFileFromPathAsync(filePath);
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine($"[CoverLoaderConverter] GetFileFromPathAsync failed for '{filePath}': {ex}");
                    Cache[filePath] = DefaultCover;
                    return;
                }

                byte[] coverBytes = null;
                
                try
                {
                    coverBytes = await CoverLoaderService.Instance.LoadCoverAsync(file);
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine($"[CoverLoaderConverter] CoverLoaderService load error for '{filePath}': {ex}");
                    coverBytes = null;
                }

                if (coverBytes == null || coverBytes.Length == 0)
                {
                    //Debug.WriteLine($"[CoverLoaderConverter] No cover bytes for '{filePath}', using DefaultCover.");
                    Cache[filePath] = DefaultCover;
                    return;
                }

                var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        using (var stream = new InMemoryRandomAccessStream())
                        {
                            using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
                            {
                                writer.WriteBytes(coverBytes);
                                await writer.StoreAsync();
                                await writer.FlushAsync();
                            }
                            stream.Seek(0);
                            await bitmap.SetSourceAsync(stream);
                        }

                        //Debug.WriteLine($"[CoverLoaderConverter] Successfully set image for '{filePath}'.");
                        // 更新缓存为已经加载的 bitmap（同引用）
                        Cache[filePath] = bitmap;
                    }
                    catch (Exception ex)
                    {
                        //Debug.WriteLine($"[CoverLoaderConverter] SetSourceAsync error for '{filePath}': {ex}");
                        Cache[filePath] = DefaultCover;
                    }
                });
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"[CoverLoaderConverter] Unexpected error for '{filePath}': {ex}");
                Cache[filePath] = DefaultCover;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}