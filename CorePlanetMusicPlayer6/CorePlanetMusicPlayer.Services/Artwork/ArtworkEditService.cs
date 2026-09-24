using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Artwork;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Core.Playlists;
using CorePlanetMusicPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Artwork
{
    public sealed class ArtworkEditService : IArtworkEditService
    {
        private readonly IArtworkRepository _artworkRepository;
        private readonly IMusicRepository _musicRepository;
        private readonly IArtistRepository _artistRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IArtworkStore _artworkStore;

        private readonly SemaphoreSlim _writeLock =
            new SemaphoreSlim(1, 1);

        public event EventHandler<ArtworkChangedEventArgs> ArtworkChanged;

        public ArtworkEditService(
            IArtworkRepository artworkRepository,
            IMusicRepository musicRepository,
            IArtistRepository artistRepository,
            IPlaylistRepository playlistRepository,
            IArtworkStore artworkStore)
        {
            Guard.NotNull(artworkRepository, nameof(artworkRepository));
            Guard.NotNull(musicRepository, nameof(musicRepository));
            Guard.NotNull(artistRepository, nameof(artistRepository));
            Guard.NotNull(playlistRepository, nameof(playlistRepository));
            Guard.NotNull(artworkStore, nameof(artworkStore));

            _artworkRepository = artworkRepository;
            _musicRepository = musicRepository;
            _artistRepository = artistRepository;
            _playlistRepository = playlistRepository;
            _artworkStore = artworkStore;
        }

        public async Task<ArtworkAssignment> ImportAsync(ArtworkOwner owner, Stream source)
        {
            Guard.NotNull(owner, nameof(owner));
            Guard.NotNull(source, nameof(source));

            bool changed = false;

            await _writeLock.WaitAsync();

            try
            {
                await ValidateOwnerAsync(owner);

                var previous = await _artworkRepository.GetByOwnerAsync(owner);

                var resourceKey = await _artworkStore.ImportAsync(owner.Kind, source);

                ArtworkAssignment assignment;

                try
                {
                    // 图片处理期间，实体可能已被其他页面删除。
                    await ValidateOwnerAsync(owner);

                    assignment = ArtworkAssignment.CreateManagedFile(owner, resourceKey, DateTimeOffset.UtcNow);

                    await _artworkRepository.UpsertAsync(assignment);

                    changed = true;
                }
                catch
                {
                    // 只清理确认没有被数据库引用的新文件。
                    await TryDeleteUnreferencedAsync(owner.Kind, resourceKey);

                    throw;
                }

                // 数据库更新成功后，才清理旧图片。
                if (previous != null && previous.SourceKind == ArtworkSourceKind.ManagedFile)
                {
                    await TryDeleteUnreferencedAsync(previous.Owner.Kind, previous.ResourceKey);
                }

                return assignment;
            }
            finally
            {
                _writeLock.Release();

                if (changed)
                {
                    RaiseArtworkChanged(owner);
                }
            }
        }

        public async Task<ArtworkAssignment> SetRemoteUriAsync(ArtworkOwner owner, string remoteUrl)
        {
            Guard.NotNull(owner, nameof(owner));

            // 当前仅开放 StreamMusic 的 URL 设置。
            if (owner.Kind != ArtworkOwnerKind.Music)
            {
                throw new InvalidOperationException("目前仅支持为流媒体音乐设置图片 URL。");
            }

            bool changed = false;

            await _writeLock.WaitAsync();

            try
            {
                // 现有方法会继续检查音乐是否存在、是否为 StreamMusic。
                await ValidateOwnerAsync(owner);

                // 通过模型工厂验证 URL；验证失败不会修改原有设置。
                var assignment = ArtworkAssignment.CreateRemoteUri(
                    owner,
                    remoteUrl,
                    DateTimeOffset.UtcNow);

                var previous = await _artworkRepository.GetByOwnerAsync(owner);

                await _artworkRepository.UpsertAsync(assignment);

                changed = true;

                // 新关联保存成功后，才清理旧的导入图片。
                if (previous != null && previous.SourceKind == ArtworkSourceKind.ManagedFile)
                {
                    await TryDeleteUnreferencedAsync(previous.Owner.Kind, previous.ResourceKey);
                }

                return assignment;
            }
            finally
            {
                _writeLock.Release();

                if (changed)
                {
                    RaiseArtworkChanged(owner);
                }
            }
        }

        public async Task ResetAsync(ArtworkOwner owner)
        {
            Guard.NotNull(owner, nameof(owner));

            bool changed = false;

            await _writeLock.WaitAsync();

            try
            {
                await ValidateOwnerAsync(owner);

                var previous = await _artworkRepository.GetByOwnerAsync(owner);

                await _artworkRepository.DeleteByOwnerAsync(owner);

                changed = previous != null;

                if (previous != null && previous.SourceKind == ArtworkSourceKind.ManagedFile)
                {
                    await TryDeleteUnreferencedAsync(previous.Owner.Kind, previous.ResourceKey);
                }
            }
            finally
            {
                _writeLock.Release();

                if (changed)
                {
                    RaiseArtworkChanged(owner);
                }
            }
        }

        private async Task ValidateOwnerAsync(ArtworkOwner owner)
        {
            switch (owner.Kind)
            {
                case ArtworkOwnerKind.Music:
                    var music = await _musicRepository.GetByIdAsync(new MusicId(owner.Id));

                    if (music == null)
                    {
                        throw new InvalidOperationException("音乐不存在。");
                    }

                    if (music.SourceType != MusicSourceType.Stream)
                    {
                        throw new InvalidOperationException("只有流媒体音乐允许设置独立封面；本地音乐需要修改音乐文件本身的封面。");
                    }

                    return;

                case ArtworkOwnerKind.Artist:
                    var artist = await _artistRepository.GetByIdAsync(new ArtistId(owner.Id));

                    if (artist == null)
                    {
                        throw new InvalidOperationException("艺术家不存在。");
                    }

                    return;

                case ArtworkOwnerKind.Playlist:
                    var playlist = await _playlistRepository.GetByIdAsync(new PlaylistId(owner.Id));

                    if (playlist == null)
                    {
                        throw new InvalidOperationException("播放列表不存在。");
                    }

                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(owner));
            }
        }

        private async Task TryDeleteUnreferencedAsync(ArtworkOwnerKind ownerKind, string resourceKey)
        {
            try
            {
                var assignments = await _artworkRepository.GetAllAsync();

                foreach (var assignment in assignments)
                {
                    if (assignment.SourceKind == ArtworkSourceKind.ManagedFile &&
                        assignment.Owner.Kind == ownerKind &&
                        string.Equals(assignment.ResourceKey, resourceKey, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }

                await _artworkStore.DeleteAsync(ownerKind, resourceKey);
            }
            catch (Exception exception)
            {
                // 无法确认引用状态或删除失败时，保留文件。
                // 后续由孤立图片清理功能处理。
                Debug.WriteLine("清理未引用的图片失败：" + exception);
            }
        }

        private void RaiseArtworkChanged(ArtworkOwner owner)
        {
            var handlers = ArtworkChanged;

            if (handlers == null)
            {
                return;
            }

            var args = new ArtworkChangedEventArgs(owner);

            foreach (EventHandler<ArtworkChangedEventArgs> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(this, args);
                }
                catch (Exception exception)
                {
                    // 数据已经保存，不能因为同步订阅处理失败而报告保存失败。
                    Debug.WriteLine("处理图片变更通知失败：" + exception);
                }
            }
        }
    }
}
