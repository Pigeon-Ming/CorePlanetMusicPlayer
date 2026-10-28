using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Artwork
{
    public sealed class ArtworkAssignment
    {
        public ArtworkOwner Owner { get; private set; }

        public ArtworkSourceKind SourceKind { get; private set; }

        public string ResourceKey { get; private set; }

        public string RemoteUrl { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }

        private ArtworkAssignment()
        {
            ResourceKey = string.Empty;
            RemoteUrl = string.Empty;
        }

        public static ArtworkAssignment CreateManagedFile(ArtworkOwner owner, string resourceKey, DateTimeOffset updatedAt)
        {
            ValidateOwner(owner);
            ValidateResourceKey(resourceKey);

            return new ArtworkAssignment
            {
                Owner = owner,
                SourceKind = ArtworkSourceKind.ManagedFile,
                ResourceKey = resourceKey,
                UpdatedAt = updatedAt
            };
        }

        public static ArtworkAssignment CreateRemoteUri(ArtworkOwner owner, string remoteUrl, DateTimeOffset updatedAt)
        {
            ValidateOwner(owner);

            var text = remoteUrl == null ? string.Empty : remoteUrl.Trim();

            Uri uri;

            if (!Uri.TryCreate(text, UriKind.Absolute, out uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Artwork URL must be an absolute HTTP or HTTPS URL.", nameof(remoteUrl));
            }

            return new ArtworkAssignment
            {
                Owner = owner,
                SourceKind = ArtworkSourceKind.RemoteUri,
                RemoteUrl = text,
                UpdatedAt = updatedAt
            };
        }

        private static void ValidateOwner(ArtworkOwner owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }
        }

        private static void ValidateResourceKey(string resourceKey)
        {
            if (string.IsNullOrWhiteSpace(resourceKey) || resourceKey == "." || resourceKey == "..")
            {
                throw new ArgumentException("Artwork resource key is invalid.", nameof(resourceKey));
            }

            // 资源键是应用生成的文件名，不接受路径或 URL。
            foreach (var character in resourceKey)
            {
                var isAllowed =
                    (character >= 'a' && character <= 'z') ||
                    (character >= 'A' && character <= 'Z') ||
                    (character >= '0' && character <= '9') ||
                    character == '-' ||
                    character == '_' ||
                    character == '.';

                if (!isAllowed)
                {
                    throw new ArgumentException("Artwork resource key contains invalid characters.", nameof(resourceKey));
                }
            }
        }
    }
}
