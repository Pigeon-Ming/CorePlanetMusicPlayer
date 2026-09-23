using CorePlanetMusicPlayer.Core.History;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Playback.Player;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Hisrory
{
    /// <summary>
    /// 跟踪当前一次播放。
    /// 调用方负责根据真实播放状态启动、暂停和结束计时。
    /// </summary>
    public sealed class PlaybackHistoryTracker
    {
        private readonly object _syncRoot = new object();

        private readonly Stopwatch _playedTime = new Stopwatch();

        private string _titleSnapshot = string.Empty;
        private string _artistNameSnapshot = string.Empty;
        private string _albumTitleSnapshot = string.Empty;

        private bool _hasSession;

        private PlaybackHistoryId _historyId;
        private MusicId _musicId;
        private DateTimeOffset _playedAt;

        private TimeSpan _musicDuration;
        private TimeSpan _lastPosition;

        public bool HasSession
        {
            get
            {
                lock (_syncRoot)
                {
                    return _hasSession;
                }
            }
        }

        /// <summary>
        /// 开始跟踪一次已经实际开始播放的歌曲。
        /// 同一条记录暂停后继续，应调用 SetPlaying(true)。
        /// </summary>
        public void Start(MusicId musicId, PlaybackPosition position, string titleSnapshot, string artistNameSnapshot, string albumTitleSnapshot)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("歌曲 ID 不能为空。", nameof(musicId));
            }

            lock (_syncRoot)
            {
                if (_hasSession)
                {
                    throw new InvalidOperationException("请先结束上一条播放记录，再开始新的记录。");
                }

                _historyId = PlaybackHistoryId.NewId();
                _musicId = musicId;
                _playedAt = DateTimeOffset.Now;

                _titleSnapshot = titleSnapshot ?? string.Empty;
                _artistNameSnapshot = artistNameSnapshot ?? string.Empty;
                _albumTitleSnapshot = albumTitleSnapshot ?? string.Empty;

                _musicDuration = TimeSpan.Zero;
                _lastPosition = TimeSpan.Zero;

                UpdatePositionCore(position);

                _hasSession = true;
                _playedTime.Restart();
            }
        }

        /// <summary>
        /// 控制有效播放时间的累计。
        /// 暂停、缓冲或其他非播放状态传入 false。
        /// </summary>
        public void SetPlaying(bool isPlaying)
        {
            lock (_syncRoot)
            {
                if (!_hasSession)
                {
                    return;
                }

                if (isPlaying)
                {
                    // Start 不会清零，重复调用也不会重复计时。
                    _playedTime.Start();
                }
                else
                {
                    _playedTime.Stop();
                }
            }
        }

        /// <summary>
        /// 更新媒体位置和总时长，不据此增加累计播放时长。
        /// </summary>
        public void UpdatePosition(PlaybackPosition position)
        {
            lock (_syncRoot)
            {
                if (!_hasSession)
                {
                    return;
                }

                UpdatePositionCore(position);
            }
        }

        /// <summary>
        /// 获取当前快照，保留本次播放过程及计时状态。
        /// 可用于暂停或挂起前保存。
        /// </summary>
        public PlaybackHistorySnapshot CaptureSnapshot(PlaybackPosition position = null)
        {
            lock (_syncRoot)
            {
                if (!_hasSession)
                {
                    return null;
                }

                UpdatePositionCore(position);

                return CreateSnapshotCore();
            }
        }

        /// <summary>
        /// 结束本次跟踪并返回最终快照。
        /// 重复调用不会再次产生同一条记录。
        /// </summary>
        public PlaybackHistorySnapshot Finish(PlaybackPosition position = null)
        {
            lock (_syncRoot)
            {
                if (!_hasSession)
                {
                    return null;
                }

                _playedTime.Stop();
                UpdatePositionCore(position);

                var snapshot = CreateSnapshotCore();

                _hasSession = false;
                _playedTime.Reset();

                _historyId = default(PlaybackHistoryId);
                _musicId = default(MusicId);
                _playedAt = default(DateTimeOffset);

                _titleSnapshot = string.Empty;
                _artistNameSnapshot = string.Empty;
                _albumTitleSnapshot = string.Empty;

                _musicDuration = TimeSpan.Zero;
                _lastPosition = TimeSpan.Zero;

                return snapshot;
            }
        }

        private void UpdatePositionCore(PlaybackPosition position)
        {
            if (position == null)
            {
                return;
            }

            // 加载过程中总时长可能暂时未知。
            // 不用未知值覆盖已经获取到的有效总时长。
            if (position.Duration > TimeSpan.Zero)
            {
                _musicDuration = position.Duration;
            }

            _lastPosition = position.Position;

            if (_musicDuration > TimeSpan.Zero &&
                _lastPosition > _musicDuration)
            {
                _lastPosition = _musicDuration;
            }
        }

        private PlaybackHistorySnapshot CreateSnapshotCore()
        {
            return new PlaybackHistorySnapshot(
                _historyId,
                _musicId,
                _playedAt,
                _musicDuration,
                _playedTime.Elapsed,
                _lastPosition,
                _titleSnapshot,
                _artistNameSnapshot,
                _albumTitleSnapshot);
        }
    }
}
