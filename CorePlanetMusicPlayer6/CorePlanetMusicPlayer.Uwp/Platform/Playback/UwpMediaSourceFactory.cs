using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace CorePlanetMusicPlayer.Uwp.Platform.Playback
{
    public sealed class UwpMediaSourceFactory
    {
        private readonly IMusicRepository _musicRepository;
        private readonly ILibraryFolderRepository _libraryFolderRepository;
        private readonly UwpStorageAccessService _storageAccessService;

        public UwpMediaSourceFactory(IMusicRepository musicRepository, ILibraryFolderRepository libraryFolderRepository, UwpStorageAccessService storageAccessService)
        {
            _musicRepository = musicRepository;
            _libraryFolderRepository = libraryFolderRepository;
            _storageAccessService = storageAccessService;
        }

        public async Task<MediaSource> CreateAsync(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                return null;
            }

            if (_musicRepository == null)
            {
                return null;
            }

            var music = await _musicRepository.GetByIdAsync(musicId);

            if (music == null)
            {
                return null;
            }

            if (music.SourceType == MusicSourceType.Local ||
                music.SourceType == MusicSourceType.Temporary)
            {
                return await CreateLocalMediaSourceAsync(music);
            }

            if (music.SourceType == MusicSourceType.Stream)
            {
                return CreateStreamMediaSource(music);
            }

            return null;
        }

        private async Task<MediaSource> CreateLocalMediaSourceAsync(Music music)
        {
            if (music == null || music.FileInfo == null)
            {
                return null;
            }

            if (_libraryFolderRepository == null ||
                _storageAccessService == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(music.FileInfo.LibraryFolderId))
            {
                return null;
            }

            var folderId = new LibraryFolderId(
                music.FileInfo.LibraryFolderId);

            if (folderId.IsEmpty)
            {
                return null;
            }

            var libraryFolder = await _libraryFolderRepository
                .GetByIdAsync(folderId);

            if (libraryFolder == null)
            {
                return null;
            }

            var file = await _storageAccessService.GetStorageFileAsync(
                music,
                libraryFolder);

            if (file == null)
            {
                return null;
            }

            return MediaSource.CreateFromStorageFile(file);
        }

        private static MediaSource CreateStreamMediaSource(Music music)
        {
            if (music == null || music.FileInfo == null)
            {
                return null;
            }

            var source = music.FileInfo.Path;

            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            Uri uri;

            if (!Uri.TryCreate(source, UriKind.Absolute, out uri))
            {
                return null;
            }

            return MediaSource.CreateFromUri(uri);
        }
    }
}
