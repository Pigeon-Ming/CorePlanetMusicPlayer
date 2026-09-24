using CorePlanetMusicPlayer.Core.Artwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Artwork
{
    public sealed class ArtworkRequest
    {
        public IReadOnlyList<ArtworkReference> Candidates { get; }

        public string DefaultResourceName { get; }

        public ArtworkRequest(IEnumerable<ArtworkReference> candidates,string defaultResourceName = "DefaultAlbumArtwork")
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            var items = new List<ArtworkReference>();

            foreach (var candidate in candidates)
            {
                if (candidate == null)
                {
                    throw new ArgumentException("候选图片来源不能为 null。", nameof(candidates));
                }

                // 默认图只能在全部候选失败后使用。
                if (candidate.SourceKind != ArtworkSourceKind.Default)
                {
                    items.Add(candidate);
                }
            }

            Candidates = items.AsReadOnly();

            DefaultResourceName =
                string.IsNullOrWhiteSpace(defaultResourceName) ? "DefaultAlbumArtwork" : defaultResourceName.Trim();
        }

        public static ArtworkRequest FromReference(ArtworkReference reference)
        {
            if (reference == null)
            {
                return new ArtworkRequest(new ArtworkReference[0]);
            }

            return new ArtworkRequest(new[] { reference }, reference.DefaultResourceName);
        }
    }
}
