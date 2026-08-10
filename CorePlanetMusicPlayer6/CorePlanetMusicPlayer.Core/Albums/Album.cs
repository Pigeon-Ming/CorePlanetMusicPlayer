using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Albums
{
    /// <summary>
    /// 专辑
    /// </summary>
    public class Album
    {
        public AlbumId Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ArtistName { get; set; } = string.Empty;
        
        public string AlbumArtistName { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;

        public int? Year { get; set; }

        public List<MusicId> MusicIds { get; set; } = new List<MusicId>();

        public TimeSpan TotalDuration { get; set; }

        public DateTimeOffset? AddedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public int MusicCount
        {
            get
            {
                return MusicIds == null ? 0 : MusicIds.Count;
            }
        }

        public bool HasTitle
        {
            get { return !string.IsNullOrWhiteSpace(Title); }
        }

        public bool HasAlbumArtist
        {
            get { return !string.IsNullOrWhiteSpace(ArtistName); }
        }

        public static Album Create(string title, string artistName, string albumArtistName, int? year)
        {
            Guard.NotNullOrWhiteSpace(title, nameof(title));

            return new Album
            {
                Id = AlbumId.NewId(),
                Title = title,
                ArtistName = artistName ?? string.Empty,
                AlbumArtistName = albumArtistName ?? string.Empty,
                Year = year,
                MusicIds = new List<MusicId>(),
                AddedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now
            };
        }

        public void AddMusic(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("Music id cannot be empty.", nameof(musicId));
            }

            if (MusicIds == null)
            {
                MusicIds = new List<MusicId>();
            }

            if (!MusicIds.Contains(musicId))
            {
                MusicIds.Add(musicId);
                UpdatedAt = DateTimeOffset.Now;
            }
        }

        public void RemoveMusic(MusicId musicId)
        {
            if (MusicIds == null)
            {
                return;
            }
            
            if (MusicIds.Remove(musicId))
            {
                UpdatedAt = DateTimeOffset.Now;
            }
        }

        public bool ContainMusic(MusicId musicId)
        {
            if (MusicIds == null)
            {
                return false;
            }
            return MusicIds.Contains(musicId);
        }

        public void SetTotalDuration(TimeSpan totalDuration)
        {
            Guard.NotNegative(totalDuration, nameof(totalDuration));

            TotalDuration = totalDuration;
            UpdatedAt = DateTimeOffset.Now;
        }
    }
}
