using CorePlanetMusicPlayer.Core.Artwork;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Services.Artwork;
using CorePlanetMusicPlayer.Uwp.Platform.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Uwp.Platform.Imaging
{
    public sealed partial class UwpArtworkLoader
    {
        private readonly UwpDispatcherService _dispatcherService;

        private readonly ArtworkMemoryCache _memoryCache = new ArtworkMemoryCache();

        private readonly SemaphoreSlim _loadSlots = new SemaphoreSlim(4, 4);

        private readonly Dictionary<string, PendingLoad> _pending = new Dictionary<string, PendingLoad>(StringComparer.Ordinal);

        private sealed class PendingLoad
        {
            public string Key;
            public string OwnerKey;
            public ArtworkReference Reference;
            public int DecodePixelSize;
            public int ConsumerCount;
            public bool Completed;

            public readonly CancellationTokenSource Cancellation = new CancellationTokenSource();

            public Task<BitmapImage> Task;
        }

        public async Task<BitmapImage> LoadAsync(ArtworkReference reference, int decodePixelSize = 256, CancellationToken cancellationToken = default(CancellationToken))
        {
            VerifyUiThread();

            cancellationToken.ThrowIfCancellationRequested();

            int size = NormalizeDecodeSize(decodePixelSize);

            reference = reference ?? ArtworkReference.Default();

            var image = await GetSharedImageAsync(reference, size, cancellationToken);

            if (image != null)
            {
                return image;
            }

            if (reference.SourceKind != ArtworkSourceKind.Default)
            {
                // 默认图片不绑定具体实体，可以被多个实体复用。
                var defaultReference = ArtworkReference.Default(reference.DefaultResourceName);

                image = await GetSharedImageAsync(defaultReference, size, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            return image ?? new BitmapImage();
        }

        public async Task<BitmapImage> LoadRequestAsync(ArtworkRequest request, int decodePixelSize = 256, CancellationToken cancellationToken = default(CancellationToken))
        {
            VerifyUiThread();

            Guard.NotNull(request, nameof(request));

            cancellationToken.ThrowIfCancellationRequested();

            int size = NormalizeDecodeSize(decodePixelSize);

            foreach (var candidate in request.Candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 直接获取实际来源的结果。
                // 这里不能调用带默认图兜底的 LoadAsync。
                var image = await GetSharedImageAsync(candidate, size, cancellationToken);

                Debug.WriteLine(
    $"[封面候选] 类型={candidate.SourceKind}，" +
    $"实体={candidate.Owner?.Id}，" +
    $"读取成功={image != null && image.PixelWidth > 0 && image.PixelHeight > 0}");

                cancellationToken.ThrowIfCancellationRequested();

                if (image != null && image.PixelWidth > 0 && image.PixelHeight > 0)
                {
                    return image;
                }
            }

            // 所有候选均失败后，才显示默认图。
            return await LoadAsync(ArtworkReference.Default(request.DefaultResourceName), size, cancellationToken);
        }

        public Task InvalidateAsync(ArtworkOwner owner)
        {
            Guard.NotNull(owner, nameof(owner));

            var ownerKey = CreateOwnerKey(owner);

            return _dispatcherService.RunAsync(() =>
            {
                _memoryCache.RemoveOwner(ownerKey);

                foreach (var request in _pending.Values.ToList())
                {
                    if (string.Equals(request.OwnerKey, ownerKey, StringComparison.Ordinal))
                    {
                        CancelRequest(request);
                    }
                }
            });
        }

        public Task ClearMemoryCacheAsync()
        {
            return _dispatcherService.RunAsync(() =>
            {
                _memoryCache.Clear();

                foreach (var request in _pending.Values.ToList())
                {
                    CancelRequest(request);
                }
            });
        }

        private async Task<BitmapImage> GetSharedImageAsync(ArtworkReference reference, int decodePixelSize, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var key = CreateCacheKey(reference, decodePixelSize);

            BitmapImage image;

            if (_memoryCache.TryGet(key, out image))
            {
                return image;
            }

            PendingLoad request;

            if (_pending.TryGetValue(key, out request))
            {
                request.ConsumerCount++;
            }
            else
            {
                request = new PendingLoad
                {
                    Key = key,
                    OwnerKey = CreateOwnerKey(reference.Owner),
                    Reference = reference,
                    DecodePixelSize = decodePixelSize,
                    ConsumerCount = 1
                };

                // 先登记，再开始执行，避免重复启动。
                _pending.Add(key, request);

                request.Task = RunLoadAsync(request);
            }

            try
            {
                return await WaitForConsumerAsync(request.Task, cancellationToken);
            }
            finally
            {
                request.ConsumerCount--;

                // 一个控件取消，不影响其他仍然等待的控件。
                if (request.ConsumerCount == 0 && !request.Completed)
                {
                    CancelRequest(request);
                }
            }
        }

        private async Task<BitmapImage> RunLoadAsync(PendingLoad request)
        {
            bool entered = false;

            var token = request.Cancellation.Token;

            try
            {
                if (request.Reference.SourceKind != ArtworkSourceKind.Default)
                {
                    // 快速掠过的列表项可在这段时间内取消。
                    await Task.Delay(50, token);
                }

                await _loadSlots.WaitAsync(token);
                entered = true;

                token.ThrowIfCancellationRequested();

                var image = await LoadSourceAsync(request.Reference, request.DecodePixelSize, token);

                token.ThrowIfCancellationRequested();

                PendingLoad current;

                // 旧任务被取消或失效后，不能写回缓存，
                // 也不能覆盖同一个键下后来建立的新任务。
                if (request.ConsumerCount > 0 && _pending.TryGetValue(request.Key, out current) && ReferenceEquals(current, request))
                {
                    var lifetime = image == null
                        ? TimeSpan.FromSeconds(5)
                        : request.Reference.SourceKind ==
                            ArtworkSourceKind.RemoteUri
                            ? TimeSpan.FromMinutes(5)
                            : TimeSpan.FromMinutes(30);

                    _memoryCache.Set(
                        request.Key,
                        request.OwnerKey,
                        image,
                        lifetime);
                }

                return image;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                // 防止已经没有等待者的任务留下未观察的异常。
                Debug.WriteLine("封面加载任务失败：" + exception);
                return null;
            }
            finally
            {
                request.Completed = true;

                RemovePendingRequest(request);

                if (entered)
                {
                    _loadSlots.Release();
                }

                request.Cancellation.Dispose();
            }
        }

        private void CancelRequest(PendingLoad request)
        {
            RemovePendingRequest(request);

            if (!request.Completed)
            {
                request.Cancellation.Cancel();
            }
        }

        private void RemovePendingRequest(PendingLoad request)
        {
            PendingLoad current;

            if (_pending.TryGetValue(request.Key, out current) &&
                ReferenceEquals(current, request))
            {
                _pending.Remove(request.Key);
            }
        }

        private static async Task<BitmapImage> WaitForConsumerAsync(Task<BitmapImage> task, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                return await task;
            }

            var canceled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (cancellationToken.Register(() => canceled.TrySetResult(true)))
            {
                await Task.WhenAny(task, canceled.Task);

                cancellationToken.ThrowIfCancellationRequested();

                return await task;
            }
        }

        private void VerifyUiThread()
        {
            if (!_dispatcherService.HasThreadAccess)
            {
                throw new InvalidOperationException("请从 UI 线程调用图片加载器。");
            }
        }

        private static int NormalizeDecodeSize(int size)
        {
            if (size <= 0 || size > 2048)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "图片请求尺寸必须在 1 到 2048 像素之间。");
            }

            // 将相近尺寸归为同一档，减少重复缓存。
            int result = 64;

            while (result < size)
            {
                result *= 2;
            }

            return result;
        }

        private static string CreateOwnerKey(ArtworkOwner owner)
        {
            if (owner == null)
            {
                return string.Empty;
            }

            return ((int)owner.Kind).ToString(CultureInfo.InvariantCulture) + ":" + owner.Id;
        }

        private static string CreateCacheKey(ArtworkReference reference, int decodePixelSize)
        {
            var parts = new[]
            {
                CreateOwnerKey(reference.Owner),

                ((int)reference.SourceKind).ToString(CultureInfo.InvariantCulture),

                reference.SourcePath ?? string.Empty,
                reference.RelativePath ?? string.Empty,
                reference.LibraryFolderId ?? string.Empty,
                reference.ResourceKey ?? string.Empty,
                reference.RemoteUrl ?? string.Empty,
                reference.DefaultResourceName ?? string.Empty,

                reference.SourceUpdatedAt.HasValue
                    ? reference.SourceUpdatedAt.Value.UtcDateTime.Ticks
                        .ToString(CultureInfo.InvariantCulture)
                    : string.Empty,

                reference.SourceSize.HasValue
                    ? reference.SourceSize.Value.ToString(
                        CultureInfo.InvariantCulture)
                    : string.Empty,

                decodePixelSize.ToString(
                    CultureInfo.InvariantCulture)
            };

            var builder = new StringBuilder();

            foreach (var part in parts)
            {
                // 长度前缀避免路径或 URL 中的分隔符造成键碰撞。
                builder.Append(
                    part.Length.ToString(CultureInfo.InvariantCulture));

                builder.Append(':');
                builder.Append(part);
            }

            return builder.ToString();
        }
    }
}
