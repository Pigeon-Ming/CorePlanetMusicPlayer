using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library
{
    public interface IMusicLibraryService
    {
        Task<IReadOnlyList<Music>> GetAllMusicAsync();

        Task<IReadOnlyList<Music>> SearchMusicAsync(string keyword);

        Task<IReadOnlyList<Album>> GetAllAlbumsAsync();

        Task<IReadOnlyList<Artist>> GetAllArtistsAsync();

        Task<IReadOnlyList<LibraryFolder>> GetAllFoldersAsync();

        Task AddFolderAsync(LibraryFolder folder);

        Task RemoveFolderAsync(LibraryFolderId folderId);

        Task<LibraryRefreshResult> RefreshAsync();

        Task<LibraryRefreshResult> RrefreshFolderAsync(LibraryFolderId folderId);
    }
}
