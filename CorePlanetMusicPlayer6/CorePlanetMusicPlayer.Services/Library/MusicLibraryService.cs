using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace CorePlanetMusicPlayer.Services.Library
{
    public sealed class MusicLibraryService : IMusicLibraryService
    {
        private readonly IMusicRepository _musicRepository;
        private readonly IAlbumRepository _albumRepository;
        private readonly IArtistRepository _artistRepository;
        private readonly ILibraryFolderRepository _libraryFolderRepository;
        private readonly ILibraryScanner _libraryScanner;
        private readonly MusicIndexBuilder _indexBuilder;
        private readonly LibraryQueryService _queryService;

        public MusicLibraryService(IMusicRepository musicRepository, IAlbumRepository albumRepository, IArtistRepository artistRepository, ILibraryFolderRepository libraryFolderRepository, ILibraryScanner libraryScanner, MusicIndexBuilder indexBuilder, LibraryQueryService queryService)
        {
            Guard.NotNull(musicRepository, nameof(musicRepository));
            Guard.NotNull(albumRepository, nameof(albumRepository));
            Guard.NotNull(artistRepository, nameof(artistRepository));
            Guard.NotNull(libraryFolderRepository, nameof(libraryFolderRepository));
            Guard.NotNull(libraryScanner, nameof(libraryScanner));

            _musicRepository = musicRepository;
            _albumRepository = albumRepository;
            _artistRepository = artistRepository;
            _libraryFolderRepository = libraryFolderRepository;
            _libraryScanner = libraryScanner;
            _indexBuilder = indexBuilder ?? new MusicIndexBuilder();
            _queryService = queryService ?? new LibraryQueryService(musicRepository, albumRepository, artistRepository, libraryFolderRepository);
        }

        public Task<IReadOnlyList<Music>> GetAllMusicAsync()
        {
            return _queryService.GetAllMusicAsync();
        }

        public Task<IReadOnlyList<Music>> SearchMusicAsync(string keyword)
        {
            return _queryService.SearchMusicAsync(keyword);
        }

        public Task<IReadOnlyList<Album>> GetAllAlbumsAsync()
        {
            return _queryService.GetAllAlbumsAsync();
        }

        public Task<IReadOnlyList<Artist>> GetAllArtistsAsync()
        {
            return _queryService.GetAllArtistsAsync();
        }

        public Task<IReadOnlyList<LibraryFolder>> GetAllFoldersAsync()
        {
            return _queryService.GetAllFoldersAsync();
        }

        public async Task AddFolderAsync(LibraryFolder folder)
        {
            Guard.NotNull(folder, nameof(folder));

            await _libraryFolderRepository.UpsertAsync(folder);
        }

        public async Task RemoveFolderAsync(LibraryFolderId folderId)
        {
            if (folderId.IsEmpty)
            {
                throw new ArgumentException("Folder ID cannot be empty.", nameof(folderId));
            }

            await _musicRepository.DeleteByLibraryFolderIdAsync(folderId);
            await _libraryFolderRepository.DeleteAsync(folderId);

            await RebuildIndexAsync();
        }

        private static List<Music> PrepareRefreshMusic(IReadOnlyList<Music> scannedMusic, IReadOnlyList<Music> existingMusic)
        {
            var existingByPath = new Dictionary<string, Music>(StringComparer.OrdinalIgnoreCase);

            foreach (var music in existingMusic)
            {
                string key = GetRelativePathKey(music);

                if (existingByPath.ContainsKey(key))
                {
                    throw new InvalidOperationException($"已有歌曲包含重复相对路径：{key}");
                }

                existingByPath.Add(key, music);
            }

            var scannedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var preparedMusic = new List<Music>();

            foreach (var scanned in scannedMusic)
            {
                string key = GetRelativePathKey(scanned);

                if (!scannedPaths.Add(key))
                {
                    throw new InvalidOperationException($"扫描结果包含重复相对路径：{key}");
                }

                Music existing;
                existingByPath.TryGetValue(key, out existing);

                var metadata = scanned.Metadata ?? MusicMetadata.Empty;

                // 新建 Music，避免直接修改扫描器返回的对象。
                var prepared = new Music
                {
                    Id = existing != null ? existing.Id : scanned.Id,

                    Title = scanned.Title,
                    ArtistName = scanned.ArtistName,
                    AlbumTitle = scanned.AlbumTitle,
                    Duration = scanned.Duration,
                    SourceType = scanned.SourceType,
                    FileInfo = scanned.FileInfo,

                    AddedAt = existing != null
                        ? existing.AddedAt
                        : scanned.AddedAt,

                    LastPlayedAt = existing != null
                        ? existing.LastPlayedAt
                        : scanned.LastPlayedAt,

                    Metadata = new MusicMetadata
                    {
                        Title = scanned.Title,
                        ArtistName = scanned.ArtistName,
                        AlbumTitle = scanned.AlbumTitle,
                        AlbumArtistName = metadata.AlbumArtistName,
                        Genre = metadata.Genre,
                        Year = metadata.Year,
                        TrackNumber = metadata.TrackNumber,
                        Composer = metadata.Composer,

                        // 当前读取器尚未读取这两个字段，先保留已有值。
                        DiscNumber = existing?.Metadata != null
                            ? existing.Metadata.DiscNumber
                            : metadata.DiscNumber,

                        Comment = existing?.Metadata != null
                            ? existing.Metadata.Comment
                            : metadata.Comment
                    }
                };

                preparedMusic.Add(prepared);
            }

            return preparedMusic;
        }

        public async Task<LibraryRefreshResult> RefreshAsync()
        {
            var result = new LibraryRefreshResult();
            var folders = await _libraryFolderRepository.GetAllAsync();

            if (folders == null || folders.Count == 0)
            {
                return result;
            }

            foreach (var folder in folders)
            {
                var folderResult = await RefreshFolderCoreAsync(folder);

                MergeResult(result, folderResult);
            }

            if (result.UpdatedFolderCount > 0)
            {
                await TryRebuildIndexAsync(result);
            }

            return result;
        }

        public async Task<LibraryRefreshResult> RefreshFolderAsync(LibraryFolderId folderId)
        {
            if (folderId.IsEmpty)
            {
                throw new ArgumentException("Folder id cannot be empty.", nameof(folderId));
            }

            var result = new LibraryRefreshResult();
            var folder = await _libraryFolderRepository.GetByIdAsync(folderId);

            if (folder == null)
            {
                result.AddError("音乐库目录不存在。");
                return result;
            }

            result = await RefreshFolderCoreAsync(folder);

            if (result.UpdatedFolderCount > 0)
            {
                await TryRebuildIndexAsync(result);
            }

            return result;
        }

        private async Task<LibraryRefreshResult> RefreshFolderCoreAsync(LibraryFolder folder)
        {
            var result = new LibraryRefreshResult();

            if (folder == null || folder.Id.IsEmpty)
            {
                result.AddError("音乐库目录无效。");
                return result;
            }

            result.AddFolder();

            LibraryScanResult scanResult;

            try
            {
                scanResult = await _libraryScanner.ScanAsync(folder);
            }
            catch (Exception ex)
            {
                result.AddError($"扫描“{folder.DisplayName}”失败：{ex.Message}");
                return result;
            }

            if (scanResult == null)
            {
                result.AddError("扫描器没有返回有效结果，已保留原歌曲。");
                return result;
            }

            result.AddScannedMusic(scanResult.Items.Count);

            if (!scanResult.IsComplete)
            {
                foreach (string error in scanResult.Errors)
                {
                    result.AddError($"扫描“{folder.DisplayName}”未完成：{error}");
                }

                if (scanResult.Errors.Count == 0)
                {
                    result.AddError("扫描未完成，已保留原歌曲。");
                }
                return result;
            }

            // 即使扫描器声称完成，也先检查返回的数据。
            var musicIds = new HashSet<MusicId>();
            string folderId = folder.Id.ToString();

            foreach (var music in scanResult.Items)
            {
                if (music == null || music.Id.IsEmpty || !musicIds.Add(music.Id) || music.SourceType != MusicSourceType.Local || music.FileInfo == null || !string.Equals( music.FileInfo.LibraryFolderId, folderId, StringComparison.Ordinal))
                {
                    result.AddError("扫描结果包含无效歌曲、重复 ID 或错误的来源，已保留原歌曲。");
                    return result;
                }
            }

            try
            {
                var existingMusic =
                    await _musicRepository.GetByLibraryFolderIdAsync(folder.Id);

                var preparedMusic = PrepareRefreshMusic(
                    scanResult.Items,
                    existingMusic);

                await _musicRepository.ReplaceByLibraryFolderIdAsync(
                    folder.Id,
                    preparedMusic);

                result.AddSavedMusic(preparedMusic.Count);
                result.AddUpdatedFolder();
            }
            catch (Exception ex)
            {
                result.AddError(
                    $"准备或保存“{folder.DisplayName}”的歌曲失败：{ex.Message}");
            }

            return result;
        }

        private async Task RebuildIndexAsync()
        {
            var allMusic = await _musicRepository.GetAllAsync();

            var existingAlbums = await _albumRepository.GetAllAsync();
            var existingArtists = await _artistRepository.GetAllAsync();

            var existingAlbumsByKey = existingAlbums.ToDictionary(album => Tuple.Create(album.Title, album.ArtistName));

            var existingArtistsByName = existingArtists.ToDictionary(artist => artist.Name, StringComparer.Ordinal);

            var albums = _indexBuilder.BuildAlbums(allMusic);

            foreach (var album in albums)
            {
                var key = Tuple.Create(album.Title, album.ArtistName);

                Album existing;

                if (existingAlbumsByKey.TryGetValue(key, out existing))
                {
                    album.Id = existing.Id;
                    album.Description = existing.Description ?? string.Empty;
                    album.AddedAt = existing.AddedAt;
                }
            }

            // 必须先恢复专辑 ID，再构建艺术家的 AlbumIds。
            var artists = _indexBuilder.BuildArtists(allMusic, albums);

            foreach (var artist in artists)
            {
                Artist existing;

                if (existingArtistsByName.TryGetValue(artist.Name, out existing))
                {
                    artist.Id = existing.Id;
                    artist.Description = existing.Description ?? string.Empty;
                    artist.AddedAt = existing.AddedAt;
                }
            }

            // 先保存本次构建结果。
            await _albumRepository.UpsertRangeAsync(albums);
            await _artistRepository.UpsertRangeAsync(artists);

            var albumIds = new HashSet<AlbumId>(albums.Select(album => album.Id));

            var artistIds = new HashSet<ArtistId>(artists.Select(artist => artist.Id));

            // 保存成功后，再清理本次索引中已不存在的分类。
            foreach (var existing in existingArtists)
            {
                if (!artistIds.Contains(existing.Id))
                {
                    await _artistRepository.DeleteAsync(existing.Id);
                }
            }

            foreach (var existing in existingAlbums)
            {
                if (!albumIds.Contains(existing.Id))
                {
                    await _albumRepository.DeleteAsync(existing.Id);
                }
            }
        }

        private static string GetRelativePathKey(Music music)
        {
            string path = music?.FileInfo?.RelativePath;

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException("歌曲缺少相对路径，无法匹配已有记录。");
            }

            string normalized = path.Replace('/', '\\');

            if (normalized.StartsWith("\\") || normalized.Contains(":"))
            {
                throw new InvalidOperationException("歌曲相对路径无效。");
            }

            foreach (string part in normalized.Split('\\'))
            {
                if (string.IsNullOrWhiteSpace(part) || part == "." || part == "..")
                {
                    throw new InvalidOperationException("歌曲相对路径无效。");
                }
            }

            return normalized;
        }

        private void MergeResult(LibraryRefreshResult target, LibraryRefreshResult source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int i = 0; i < source.FolderCount; i++)
            {
                target.AddFolder();
            }

            target.AddScannedMusic(source.ScannedMusicCount);
            target.AddSavedMusic(source.SavedMusicCount);
            target.AddSkippedMusic(source.SkippedMusicCount);

            for (int i = 0; i < source.Errors.Count; i++)
            {
                target.AddError(source.Errors[i]);
            }

            for (int i = 0; i < source.UpdatedFolderCount; i++)
            {
                target.AddUpdatedFolder();
            }
        }

        private async Task TryRebuildIndexAsync(LibraryRefreshResult result)
        {
            try
            {
                await RebuildIndexAsync();
            }
            catch (Exception ex)
            {
                result.AddError($"歌曲更新已保存，但专辑和艺术家索引重建失败：" + $"{ex.Message}。分类数据可能不完整，请重新刷新。");
            }
        }
    }
}
