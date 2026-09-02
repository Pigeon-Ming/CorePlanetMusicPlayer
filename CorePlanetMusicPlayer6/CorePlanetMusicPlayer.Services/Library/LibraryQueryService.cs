using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library
{
    public sealed class LibraryQueryService
    {
        private readonly IMusicRepository _musicRepository;
        private readonly IAlbumRepository _albumRepository;
        private readonly IArtistRepository _artistRepository;
        private readonly ILibraryFolderRepository _libraryFolderRepository;

        public LibraryQueryService(IMusicRepository musicRepository, IAlbumRepository albumRepository, IArtistRepository artistRepository, ILibraryFolderRepository libraryFolderRepository)
        {
            _musicRepository = musicRepository;
            _albumRepository = albumRepository;
            _artistRepository = artistRepository;
            _libraryFolderRepository = libraryFolderRepository;
        }

        public Task<IReadOnlyList<Music>> GetAllMusicAsync()
        {
            return _musicRepository.GetAllAsync();
        }

        public Task<IReadOnlyList<Music>> SearchMusicAsync(string keyword)
        {
            return _musicRepository.SearchAsync(keyword);
        }

        public Task<IReadOnlyList<Music>> GetMusicByFolderAsync(LibraryFolderId folderId)
        {
            return _musicRepository.GetByLibraryFolderIdAsync(folderId);
        }

        public Task<IReadOnlyList<Album>> GetAllAlbumsAsync()
        {
            return _albumRepository.GetAllAsync();
        }

        public Task<IReadOnlyList<Artist>> GetAllArtistsAsync()
        {
            return _artistRepository.GetAllAsync();
        }

        public Task<IReadOnlyList<LibraryFolder>> GetAllFoldersAsync()
        {
            return _libraryFolderRepository.GetAllAsync();
        }
    }
}
