using CorePlanetMusicPlayer.Models.TagLibModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.App
{
    public class CoverLoaderService
    {
        // 弱引用缓存：key=歌曲文件路径，value=封面的byte[]（弱引用，不阻碍GC）
        private readonly ConcurrentDictionary<string, WeakReference<byte[]>> _coverCache = new ConcurrentDictionary<string, WeakReference<byte[]>>();

        // 避免重复加载同一首歌的封面（防止并发请求）
        private readonly ConcurrentDictionary<string, Task<byte[]>> _loadingTasks = new ConcurrentDictionary<string, Task<byte[]>>();

        public static CoverLoaderService Instance { get; } = new CoverLoaderService();

        public async Task<byte[]> LoadCoverAsync(StorageFile storageFile)
        {
            if (storageFile == null)
                return null;

            // 1. 先查缓存（弱引用）
            if (_coverCache.TryGetValue(storageFile.Path, out var weakRef) && weakRef.TryGetTarget(out var cachedCover))
            {
                return cachedCover;
            }

            string filePath = storageFile.Path;

            // 2. 避免并发加载同一文件
            var loadTask = _loadingTasks.GetOrAdd(filePath, key => Task.Run(async () =>
            {
                var file = TagLibHelper.GetTagLibFile(storageFile);
                return await TagLibHelper.GetCoverByteArrayAsync(file);
            }));

            try
            {
                var cover = await loadTask;
                // 5. 存入弱引用缓存（GC可回收未使用的封面）
                if (cover != null)
                {
                    _coverCache[filePath] = new WeakReference<byte[]>(cover);
                }
                return cover;
            }
            finally
            {
                // 6. 加载完成后移除任务标记
                _loadingTasks.TryRemove(filePath, out _);
            }
        }
    }
}
