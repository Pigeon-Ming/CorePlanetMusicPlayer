using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Mapping
{
    public static class LibraryFolderDataMapper
    {
        public static LibraryFolder ToModel(LibraryFolderEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new LibraryFolder
            {
                Id = new LibraryFolderId(entity.Id),
                DisplayName = entity.DisplayName ?? string.Empty,
                Path = entity.Path ?? string.Empty,
                AccessKey = entity.AccessKey ?? string.Empty,
                AccessKind = (LibraryFolderAccessKind)entity.AccessKind,
                AddedAt = DataValueConverter.FromUnixTimeMilliseconds(entity.AddedAtUnixTimeMilliseconds),
                UpdatedAt = DataValueConverter.FromUnixTimeMilliseconds(entity.UpdatedAtUnixTimeMilliseconds)
            };
        }

        public static LibraryFolderEntity ToEntity(LibraryFolder folder)
        {
            if (folder == null)
            {
                return null;
            }

            return new LibraryFolderEntity
            {
                Id = folder.Id.ToString(),
                DisplayName = folder.DisplayName ?? string.Empty,
                Path = folder.Path ?? string.Empty,
                AccessKey = folder.AccessKey ?? string.Empty,
                AccessKind = (int)folder.AccessKind,
                AddedAtUnixTimeMilliseconds = folder.AddedAt.ToUnixTimeMilliseconds(),
                UpdatedAtUnixTimeMilliseconds = folder.UpdatedAt.ToUnixTimeMilliseconds()
            };
        }
    }
}
