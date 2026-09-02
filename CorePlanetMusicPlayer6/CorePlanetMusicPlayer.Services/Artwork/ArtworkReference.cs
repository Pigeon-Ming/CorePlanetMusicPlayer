using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Artwork
{
    public sealed class ArtworkReference
    {
        public ArtworkSourceKind SourceKind { get; private set; }

        public MusicId? MusicId { get; private set; }

        public string SourcePath { get; private set; }

        public string RelativePath { get; private set; }

        public string LibraryFolderId { get; private set; }

        public string CacheKey { get; private set; }

        public string DefaultResourceName { get; private set; }

        public bool CanUseCache { get; private set; }

        public bool CanUseEmbedded { get; private set; }

        public bool HasMusic
        {
            get
            {
                return MusicId.HasValue && !MusicId.Value.IsEmpty;
            }
        }

        public bool HasSourcePath
        {
            get { return !string.IsNullOrWhiteSpace(SourcePath); }
        }

        public bool HasRelativePath
        {
            get { return !string.IsNullOrWhiteSpace(RelativePath); }
        }

        public bool HasLibraryFolder
        {
            get { return !string.IsNullOrWhiteSpace(LibraryFolderId); }
        }

        public bool IsDefault
        {
            get { return SourceKind == ArtworkSourceKind.Default; }
        }

        public bool IsAuto
        {
            get { return SourceKind == ArtworkSourceKind.Auto; }
        }

        public bool IsEmbedded
        {
            get { return SourceKind == ArtworkSourceKind.Embedded; }
        }

        public bool IsCache
        {
            get { return SourceKind == ArtworkSourceKind.Cache; }
        }

        public bool IsFile
        {
            get { return SourceKind == ArtworkSourceKind.File; }
        }

        private ArtworkReference()
        {
            SourceKind = ArtworkSourceKind.None;
            SourcePath = string.Empty;
            RelativePath = string.Empty;
            LibraryFolderId = string.Empty;
            CacheKey = string.Empty;
            DefaultResourceName = string.Empty;
        }

        public static ArtworkReference None()
        {
            return new ArtworkReference
            {
                SourceKind = ArtworkSourceKind.None
            };
        }

        public static ArtworkReference CreateAuto(Music music)
        {
            if (music == null || music.Id.IsEmpty)
            {
                return Default();
            }

            var reference = new ArtworkReference
            {
                SourceKind = ArtworkSourceKind.Auto,
                MusicId = music.Id,
                CacheKey = CreateCacheKey(music.Id),
                DefaultResourceName = "DefaultAlbumArtwork",
                CanUseCache = true,
                CanUseEmbedded = true
            };

            if (music.FileInfo != null)
            {
                reference.SourcePath = music.FileInfo.Path ?? string.Empty;
                reference.RelativePath = music.FileInfo.RelativePath ?? string.Empty;
                reference.LibraryFolderId = music.FileInfo.LibraryFolderId ?? string.Empty;
            }

            return reference;
        }

        public static ArtworkReference Embedded(Music music)
        {
            if (music == null || music.Id.IsEmpty)
            {
                return Default();
            }

            var reference = new ArtworkReference
            {
                SourceKind = ArtworkSourceKind.Embedded,
                MusicId = music.Id,
                CacheKey = CreateCacheKey(music.Id),
                DefaultResourceName = "DefaultAlbumArtwork",
                CanUseCache = false,
                CanUseEmbedded = true
            };

            if (music.FileInfo != null)
            {
                reference.SourcePath = music.FileInfo.Path ?? string.Empty;
                reference.RelativePath = music.FileInfo.RelativePath ?? string.Empty;
                reference.LibraryFolderId = music.FileInfo.LibraryFolderId ?? string.Empty;
            }

            return reference;
        }

        public static ArtworkReference Cache(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                return Default();
            }

            return new ArtworkReference
            {
                SourceKind = ArtworkSourceKind.Cache,
                MusicId = musicId,
                CacheKey = CreateCacheKey(musicId),
                DefaultResourceName = "DefaultAlbumArtwork",
                CanUseCache = true,
                CanUseEmbedded = false
            };
        }

        public static ArtworkReference File(MusicId musicId, string sourcePath)
        {
            if (musicId.IsEmpty || string.IsNullOrWhiteSpace(sourcePath))
            {
                return Default();
            }

            return new ArtworkReference
            {
                SourceKind = ArtworkSourceKind.File,
                MusicId = musicId,
                SourcePath = sourcePath,
                CacheKey = CreateCacheKey(musicId),
                DefaultResourceName = "DefaultAlbumArtwork",
                CanUseCache = false,
                CanUseEmbedded = false
            };
        }

        public static ArtworkReference Default()
        {
            return new ArtworkReference
            {
                SourceKind = ArtworkSourceKind.Default,
                DefaultResourceName = "DefaultAlbumArtwork",
                CanUseCache = false,
                CanUseEmbedded = false
            };
        }

        public static ArtworkReference Default(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                return Default();
            }

            return new ArtworkReference
            {
                SourceKind = ArtworkSourceKind.Default,
                MusicId = musicId,
                CacheKey = CreateCacheKey(musicId),
                DefaultResourceName = "DefaultAlbumArtwork",
                CanUseCache = false,
                CanUseEmbedded = false
            };
        }

        private static string CreateCacheKey(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                return string.Empty;
            }

            return "music-artwork-" + musicId.ToString();
        }
    }
}
