using CorePlanetMusicPlayer.Core.Library;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library
{
    public interface IMusicLibraryService
    {
        Task<IReadOnlyList<LibraryFolder>> GetAllFoldersAsync();

        Task AddFolderAsync(LibraryFolder folder);

        Task RemoveFolderAsync(LibraryFolderId folderId);

        Task<LibraryRefreshResult> RefreshAsync();

        Task<LibraryRefreshResult> RefreshFolderAsync(LibraryFolderId folderId);
    }
}
