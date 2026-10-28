using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Uwp.Platform.Imaging
{
    public sealed class UwpRemoteArtworkLoader
    {
        private const long MaxDownloadBytes = 20L * 1024 * 1024;

        private static readonly HttpClient Client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30),
            MaxResponseContentBufferSize = MaxDownloadBytes
        };

        private readonly UwpThumbnailLoader _thumbnailLoader;

        public UwpRemoteArtworkLoader(UwpThumbnailLoader thumbnailLoader)
        {
            Guard.NotNull(thumbnailLoader, nameof(thumbnailLoader));

            _thumbnailLoader = thumbnailLoader;
        }

        public async Task<BitmapImage> LoadAsync(string remoteUrl, int decodePixelSize = 256, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (decodePixelSize <= 0 || decodePixelSize > 2048)
            {
                throw new ArgumentOutOfRangeException(nameof(decodePixelSize));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Uri uri;

            if (!Uri.TryCreate(remoteUrl, UriKind.Absolute, out uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return null;
            }

            try
            {
                using (var response = await Client.GetAsync(uri, HttpCompletionOption.ResponseContentRead, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();

                    var bytes = await response.Content.ReadAsByteArrayAsync();

                    cancellationToken.ThrowIfCancellationRequested();

                    if (bytes.Length == 0 || bytes.LongLength > MaxDownloadBytes)
                    {
                        return null;
                    }

                    using (var stream = new MemoryStream(bytes, false))
                    using (var randomStream = stream.AsRandomAccessStream())
                    {
                        var image = await _thumbnailLoader.LoadArtworkStreamAsync(randomStream, decodePixelSize);

                        cancellationToken.ThrowIfCancellationRequested();

                        return image;
                    }
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                // 包括断网、HTTP 错误、下载超时、超限和解码失败。
                Debug.WriteLine("读取网络封面失败：" + exception);

                return null;
            }
        }
    }
}
