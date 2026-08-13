using CorePlanetMusicPlayer.Core.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Repositories
{
    public interface ILibraryFolderRepository
    {
        Task<IReadOnlyList<LibraryFolder>> GetAllAsync();

        Task<LibraryFolder> GetByIdAsync(LibraryFolderId id);

        Task UpsertAsync(LibraryFolder folder);

        Task DeleteAsync(LibraryFolderId id);

        Task ClearAsync();
    }
}
