using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Albums
{
    public sealed class AlbumDetails
    {
        public Album Album { get; }

        public IReadOnlyList<Music> MusicItems { get; }

        public IReadOnlyList<MusicId> MissingMusicIds { get; }

        public AlbumDetails(Album album, IEnumerable<Music> musicItems, IEnumerable<MusicId> missingMusicIds)
        {
            Guard.NotNull(album, nameof(album));
            Guard.NotNull(musicItems, nameof(musicItems));
            Guard.NotNull(missingMusicIds, nameof(missingMusicIds));

            Album = album;
            MusicItems = musicItems.ToList().AsReadOnly();
            MissingMusicIds = missingMusicIds.ToList().AsReadOnly();
        }
    }
}
