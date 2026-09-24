using CorePlanetMusicPlayer.Core.Artwork;
using CorePlanetMusicPlayer.Services.Artwork;
using CorePlanetMusicPlayer6.Composition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls.Common
{
    public sealed partial class ArtworkImageControl : UserControl
    {
        private bool _isLoaded;
        private int _loadVersion;

        private CancellationTokenSource _loadCancellation;

        public ArtworkImageControl()
        {
            InitializeComponent();
        }

        public ArtworkRequest Request
        {
            get => (ArtworkRequest)GetValue(RequestProperty);
            set => SetValue(RequestProperty, value);
        }

        public static readonly DependencyProperty RequestProperty =
            DependencyProperty.Register(
                nameof(Request),
                typeof(ArtworkRequest),
                typeof(ArtworkImageControl),
                new PropertyMetadata(
                    null,
                    OnArtworkInputChanged));

        public int DecodePixelSize
        {
            get => (int)GetValue(DecodePixelSizeProperty);
            set => SetValue(DecodePixelSizeProperty, value);
        }

        public static readonly DependencyProperty DecodePixelSizeProperty =
            DependencyProperty.Register(
                nameof(DecodePixelSize),
                typeof(int),
                typeof(ArtworkImageControl),
                new PropertyMetadata(
                    256,
                    OnArtworkInputChanged));

        private static async void OnArtworkInputChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = (ArtworkImageControl)sender;

            await control.RefreshAsync();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;

            await RefreshAsync();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = false;

            CancelCurrentLoad();
            ClearImage();
        }

        /// <summary>
        /// 使用当前 Reference 重新请求图片。
        /// 必须在 UI 线程调用；仍然会使用加载器缓存。
        /// </summary>
        public async Task RefreshAsync()
        {
            CancelCurrentLoad();
            ClearImage();

            if (!_isLoaded)
            {
                return;
            }

            int version = _loadVersion;

            var cancellation = new CancellationTokenSource();
            var token = cancellation.Token;

            _loadCancellation = cancellation;

            try
            {
                var request = Request ?? ArtworkRequest.FromReference(null);
                int decodePixelSize = DecodePixelSize;

                if (decodePixelSize <= 0 || decodePixelSize > 2048)
                {
                    throw new ArgumentOutOfRangeException(nameof(DecodePixelSize), "图片请求尺寸必须在 1 到 2048 像素之间。");
                }

                var loader = AppRuntime.Services?.ArtworkLoader;

                if (loader == null)
                {
                    throw new InvalidOperationException("图片加载服务尚未就绪。");
                }

                var defaultImage = await loader.LoadAsync(ArtworkReference.Default(request.DefaultResourceName), decodePixelSize, token);

                if (!CanApplyResult(version, token))
                {
                    return;
                }

                ApplyImage(defaultImage);

                if (request.Candidates.Count == 0)
                {
                    return;
                }

                var image = await loader.LoadRequestAsync(request, decodePixelSize, token);

                if (!CanApplyResult(version, token))
                {
                    return;
                }

                ApplyImage(image);
            }
            catch (OperationCanceledException)
                when (token.IsCancellationRequested)
            {
                Debug.WriteLine(
                    $"[封面] 控件主动取消：" +
                    $"请求版本={version}，当前版本={_loadVersion}");
            }
            catch (OperationCanceledException exception)
            {
                Debug.WriteLine(
                    $"[封面] 控件未主动取消，但底层加载已取消：" +
                    $"请求版本={version}，当前版本={_loadVersion}，" +
                    $"已加载={_isLoaded}");

                Debug.WriteLine(exception.ToString());
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    "显示封面失败：" + exception);

                // 保留已经显示的默认图片或占位内容。
            }
            finally
            {
                if (ReferenceEquals(_loadCancellation, cancellation))
                {
                    _loadCancellation = null;
                }

                // 由创建本次请求的方法负责释放。
                cancellation.Dispose();
            }
        }

        private bool CanApplyResult(int version, CancellationToken token)
        {
            return _isLoaded && version == _loadVersion && !token.IsCancellationRequested;
        }

        private void CancelCurrentLoad()
        {
            // 即使底层操作无法立即取消，
            // 旧结果也会因版本不一致而被丢弃。
            _loadVersion++;

            var cancellation = _loadCancellation;
            _loadCancellation = null;

            cancellation?.Cancel();
        }

        private void ClearImage()
        {
            ArtworkImage.Source = null;
            PlaceholderBorder.Visibility = Visibility.Visible;
        }

        private void ApplyImage(BitmapImage image)
        {
            if (image == null || image.PixelWidth <= 0 || image.PixelHeight <= 0)
            {
                return;
            }

            ArtworkImage.Source = image;
            PlaceholderBorder.Visibility = Visibility.Collapsed;
        }
    }
}
