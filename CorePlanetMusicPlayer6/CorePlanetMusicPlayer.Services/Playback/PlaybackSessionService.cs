using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Playback.Modes;
using CorePlanetMusicPlayer.Playback.Player;
using CorePlanetMusicPlayer.Playback.Queue;
using CorePlanetMusicPlayer.Services.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Playback
{
    public sealed class PlaybackSessionService
    {
        private readonly IPlaybackService _playbackService;
        private readonly ISettingsService _settingsService;
        private readonly IPlaybackSessionStore _store;

        private readonly SemaphoreSlim _saveGate = new SemaphoreSlim(1, 1);

        private bool _initialized;

        public bool RestoreOnStartup { get; private set; }

        public string LastError { get; private set; }

        public PlaybackSessionService(IPlaybackService playbackService, ISettingsService settingsService, IPlaybackSessionStore store)
        {
            _playbackService = playbackService ?? throw new ArgumentNullException(nameof(playbackService));

            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        // 由应用启动流程调用一次，完成前不开放界面操作。
        public async Task InitializeAsync()
        {
            if (_initialized)
            {
                return;
            }

            var settings = await _settingsService.LoadAsync();
            RestoreOnStartup = settings.Playback.RestoreQueueOnStartup;

            if (RestoreOnStartup)
            {
                try
                {
                    var data = await _store.LoadAsync();

                    if (data != null)
                    {
                        var snapshot = CreateQueueSnapshot(data);

                        await _playbackService.RestoreQueueAsync(snapshot, (PlaybackMode)data.Mode);
                    }
                }
                catch (Exception ex)
                {
                    LastError = $"恢复上次播放队列失败：{ex.Message}";
                    Debug.WriteLine(ex);
                }
            }

            // 先恢复，再订阅，避免启动时用空队列覆盖旧快照。
            _playbackService.QueueChanged += PlaybackService_Changed;
            _playbackService.PlaybackModeChanged += PlaybackService_Changed;

            _initialized = true;
        }

        public async Task SetRestoreOnStartupAsync(bool enabled)
        {
            await _saveGate.WaitAsync();

            try
            {
                if (!_initialized)
                {
                    throw new InvalidOperationException(
                        "播放会话服务尚未初始化。");
                }

                if (enabled)
                {
                    // 启用时先保存当前队列。
                    await _store.SaveAsync(Capture());
                }

                // 读取最新设置，避免使用启动时的旧设置对象覆盖其他设置。
                var settings = await _settingsService.LoadAsync();
                settings.Playback.RestoreQueueOnStartup = enabled;

                await _settingsService.SaveAsync(settings);

                RestoreOnStartup = enabled;
                LastError = null;
            }
            finally
            {
                _saveGate.Release();
            }
        }

        public async Task SaveAsync()
        {
            await _saveGate.WaitAsync();

            try
            {
                if (!_initialized || !RestoreOnStartup)
                {
                    return;
                }

                // 获得保存权限之后才取快照，避免排队的旧数据覆盖新数据。
                await _store.SaveAsync(Capture());

                LastError = null;
            }
            catch (Exception ex)
            {
                LastError = $"保存播放队列失败：{ex.Message}";
                throw;
            }
            finally
            {
                _saveGate.Release();
            }
        }

        private async void PlaybackService_Changed(object sender, EventArgs e)
        {
            try
            {
                await SaveAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private PlaybackSessionData Capture()
        {
            var snapshot = _playbackService.QueueSnapshot;

            return new PlaybackSessionData
            {
                Version = 1,
                Mode = (int)_playbackService.State.Mode,
                CurrentIndex = snapshot.CurrentIndex,
                ShuffleItemIds = new List<string>(snapshot.ShuffleItemIds),

                Items = snapshot.Items.Select(item =>
                    new PlaybackSessionItemData
                    {
                        Id = item.Id,
                        MusicId = item.MusicId.Value,
                        Order = item.Order
                    }).ToList()
            };
        }

        private static PlaybackQueueSnapshot CreateQueueSnapshot(PlaybackSessionData data)
        {
            if (data.Version != 1)
            {
                throw new InvalidOperationException("不支持的播放队列文件版本。");
            }

            if (data.Items == null || data.ShuffleItemIds == null || data.Items.Any(item => item == null))
            {
                throw new InvalidOperationException("播放队列文件内容不完整。");
            }

            return new PlaybackQueueSnapshot
            {
                CurrentIndex = data.CurrentIndex,

                Items = data.Items.Select(item =>
                    PlaybackQueueItem.Restore(item.Id, new MusicId(item.MusicId), item.Order)).ToList(),

                ShuffleItemIds = new List<string>(data.ShuffleItemIds)
            };
        }
    }
}
