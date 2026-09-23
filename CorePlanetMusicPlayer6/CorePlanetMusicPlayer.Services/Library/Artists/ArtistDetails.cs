using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Artists
{
    public sealed class ArtistDetails
    {
        public Artist Artist { get; }

        public IReadOnlyList<Music> MusicItems { get; }

        public IReadOnlyList<Album> Albums { get; }

        public IReadOnlyList<MusicId> MissingMusicIds { get; }

        public IReadOnlyList<AlbumId> MissingAlbumIds { get; }

        public ArtistDetails(Artist artist, IEnumerable<Music> musicItems, IEnumerable<Album> albums, IEnumerable<MusicId> missingMusicIds, IEnumerable<AlbumId> missingAlbumIds)
        {
            Guard.NotNull(artist, nameof(artist));
            Guard.NotNull(musicItems, nameof(musicItems));
            Guard.NotNull(albums, nameof(albums));
            Guard.NotNull(missingMusicIds, nameof(missingMusicIds));
            Guard.NotNull(missingAlbumIds, nameof(missingAlbumIds));

            Artist = artist;
            MusicItems = musicItems.ToList().AsReadOnly();
            Albums = albums.ToList().AsReadOnly();
            MissingMusicIds = missingMusicIds.ToList().AsReadOnly();
            MissingAlbumIds = missingAlbumIds.ToList().AsReadOnly();
        }
    }
}
