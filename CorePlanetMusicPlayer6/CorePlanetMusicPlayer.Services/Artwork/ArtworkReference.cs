using CorePlanetMusicPlayer.Core.Artwork;
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

        public ArtworkOwner Owner { get; private set; }

        public string SourcePath { get; private set; }

        public string RelativePath { get; private set; }

        public string LibraryFolderId { get; private set; }

        public string ResourceKey { get; private set; }

        public string RemoteUrl { get; private set; }

        public DateTimeOffset? SourceUpdatedAt { get; private set; }

        public long? SourceSize { get; private set; }

        public string DefaultResourceName { get; private set; }

        private ArtworkReference()
        {
            SourceKind = ArtworkSourceKind.Default;
            SourcePath = string.Empty;
            RelativePath = string.Empty;
            LibraryFolderId = string.Empty;
            ResourceKey = string.Empty;
            RemoteUrl = string.Empty;
            DefaultResourceName = "DefaultAlbumArtwork";
        }

        public static ArtworkReference FromMusicFile(Music music, string defaultResourceName = "DefaultAlbumArtwork")
        {
            if (music == null)
            {
                throw new ArgumentNullException(nameof(music));
            }

            if (music.Id.IsEmpty)
            {
                throw new ArgumentException("Music id cannot be empty.", nameof(music));
            }

            if (music.SourceType != MusicSourceType.Local && music.SourceType != MusicSourceType.Temporary)
            {
                throw new ArgumentException("Music must have a local file source.", nameof(music));
            }

            var owner = new ArtworkOwner(ArtworkOwnerKind.Music, music.Id.ToString());

            var fileInfo = music.FileInfo;

            if (fileInfo == null || (!fileInfo.HasPath && !(fileInfo.HasRelativePath && fileInfo.HasLibraryFolder)))
            {
                return Default(defaultResourceName, owner);
            }

            return new ArtworkReference
            {
                SourceKind = ArtworkSourceKind.MusicFile,
                Owner = owner,
                SourcePath = fileInfo.Path ?? string.Empty,
                RelativePath = fileInfo.RelativePath ?? string.Empty,
                LibraryFolderId = fileInfo.LibraryFolderId ?? string.Empty,
                SourceUpdatedAt = fileInfo.LastModifiedAt,
                SourceSize = fileInfo.Size,
                DefaultResourceName = NormalizeDefaultResourceName(defaultResourceName)
            };
        }

        public static ArtworkReference FromAssignment(ArtworkAssignment assignment, string defaultResourceName = "DefaultAlbumArtwork")
        {
            if (assignment == null)
            {
                throw new ArgumentNullException(nameof(assignment));
            }

            return new ArtworkReference
            {
                SourceKind = assignment.SourceKind,
                Owner = assignment.Owner,
                ResourceKey = assignment.ResourceKey,
                RemoteUrl = assignment.RemoteUrl,
                SourceUpdatedAt = assignment.UpdatedAt,
                DefaultResourceName = NormalizeDefaultResourceName(defaultResourceName)
            };
        }

        public static ArtworkReference Default(string defaultResourceName = "DefaultAlbumArtwork", ArtworkOwner owner = null)
        {
            return new ArtworkReference
            {
                SourceKind = ArtworkSourceKind.Default,
                Owner = owner,
                DefaultResourceName = NormalizeDefaultResourceName(defaultResourceName)
            };
        }

        private static string NormalizeDefaultResourceName(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? "DefaultAlbumArtwork" : name.Trim();
        }
    }
}
