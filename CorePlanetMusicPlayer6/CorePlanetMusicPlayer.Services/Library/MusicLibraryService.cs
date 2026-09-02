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

            _musicRepository = musicRepository;
            _albumRepository = albumRepository;
            _artistRepository = artistRepository;
            _libraryFolderRepository = libraryFolderRepository;
            _libraryScanner = libraryScanner ?? new LibraryScanner();
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

        public async Task<LibraryRefreshResult> RefreshAsync()
        {
            var result = new LibraryRefreshResult();
            var folders = await _libraryFolderRepository.GetAllAsync();

            if (folders == null || folders.Count == 0)
            {
                await RebuildIndexAsync();
                return result;
            }

            foreach (var folder in folders)
            {
                var folderResult = await RefreshFolderCoreAsync(folder);

                MergeResult(result, folderResult);
            }

            await RebuildIndexAsync();

            return result;
        }

        public async Task<LibraryRefreshResult> RrefreshFolderAsync(LibraryFolderId folderId)
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

            await RebuildIndexAsync();

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

            IReadOnlyList<Music> scannedMusicList;

            try
            {
                scannedMusicList = await _libraryScanner.ScanAsync(folder);
            }
            catch (Exception ex)
            {
                result.AddError(ex.Message);
                return result;
            }

            if (scannedMusicList == null)
            {
                result.AddScannedMusic(0);
                return result;
            }

            result.AddScannedMusic(scannedMusicList.Count);

            await _musicRepository.DeleteByLibraryFolderIdAsync(folder.Id);

            var validMusicList = FilterValidMusic(scannedMusicList);

            await _musicRepository.UpsertRangeAsync(validMusicList);

            result.AddSavedMusic(validMusicList.Count);
            result.AddSkippedMusic(scannedMusicList.Count - validMusicList.Count);

            return result;
        }

        private async Task RebuildIndexAsync()
        {
            var allMusic = await _musicRepository.GetAllAsync();

            var albums = _indexBuilder.BuildAlbums(allMusic);
            var artists = _indexBuilder.BuildArtists(allMusic, albums);

            await _albumRepository.ClearAsync();
            await _artistRepository.ClearAsync();

            await _albumRepository.UpsertRangeAsync(albums);
            await _artistRepository.UpsertRangeAsync(artists);
        }

        private List<Music> FilterValidMusic(IReadOnlyList<Music> musicList)
        {
            var result = new List<Music>();

            if (musicList == null)
            {
                return result;
            }

            foreach (var music in musicList)
            {
                if (music == null)
                {
                    continue;
                }

                if (music.Id.IsEmpty)
                {
                    continue;
                }

                result.Add(music);
            }

            return result;
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
        }
    }
}
