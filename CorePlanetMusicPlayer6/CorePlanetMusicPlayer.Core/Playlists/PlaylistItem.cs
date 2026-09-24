using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Playlists
{
    public sealed class PlaylistItem
    {
        public string Id { get; set; } = string.Empty;

        public MusicId MusicId { get; set; }

        public string TitleSnapshot { get; set; } = string.Empty;

        public string ArtistNameSnapshot { get; set; } = string.Empty;

        public string AlbumTitleSnapshot { get; set; } = string.Empty;

        public int Order { get; set; }

        public DateTimeOffset AddedAt { get; set; }

        public static PlaylistItem Create(Music.Music music, int order)
        {
            Guard.NotNull(music, nameof(music));

            if (music.Id.IsEmpty)
            {
                throw new ArgumentException("歌曲 ID 不能为空。", nameof(music));
            }

            Guard.NotNegative(order, nameof(order));

            return new PlaylistItem
            {
                Id = EntityId.New(),
                MusicId = music.Id,

                TitleSnapshot = music.Title ?? string.Empty,
                ArtistNameSnapshot = music.ArtistName ?? string.Empty,
                AlbumTitleSnapshot = music.AlbumTitle ?? string.Empty,

                Order = order,
                AddedAt = DateTimeOffset.Now
            };
        }
    }
}
