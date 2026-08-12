using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Artists
{
    /// <summary>
    /// 艺术家
    /// </summary>
    public class Artist
    {
        public ArtistId Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string SortName { get; set; } = string.Empty;

        public List<MusicId> MusicIds { get; set; } = new List<MusicId>();

        public List<AlbumId> AlbumIds { get; set; } = new List<AlbumId>();

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

        public int AlbumCount
        {
            get
            {
                return AlbumIds == null ? 0 : AlbumIds.Count;
            }
        }

        public bool HasName
        {
            get { return !string.IsNullOrWhiteSpace(Name); }
        }

        public string DisplayName
        {
            get
            {
                return HasName ? Name : "未知艺术家";
            }
        }

        public static Artist Create(string name)
        {
            Guard.NotNullOrWhiteSpace(name, nameof(name));

            return new Artist
            {
                Id = ArtistId.NewId(),
                Name = name,
                SortName = name,
                MusicIds = new List<MusicId>(),
                AlbumIds = new List<AlbumId>(),
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

            if (MusicIds.Contains(musicId))
            {
                MusicIds.Remove(musicId);
                UpdatedAt = DateTimeOffset.Now;
            }
        }

        public bool ContainsMusic(MusicId musicId)
        {
            if (MusicIds == null)
            {
                return false;
            }

            return MusicIds.Contains(musicId);
        }

        public void AddAlbum (AlbumId albumId)
        {
            if (albumId.IsEmpty)
            {
                throw new ArgumentException("Album id cannot be empty.", nameof(albumId));
            }

            if (AlbumIds == null)
            {
                AlbumIds = new List<AlbumId>();
            }

            if (!AlbumIds.Contains(albumId))
            {
                AlbumIds.Add(albumId);
                UpdatedAt = DateTimeOffset.Now;
            }
        }

        public void RemoveAlbum(AlbumId albumId)
        {
            if (AlbumIds == null)
            {
                return;
            }

            if (AlbumIds.Remove(albumId))
            {
                UpdatedAt = DateTimeOffset.Now;
            }
        }

        public bool ContainsAlbum(AlbumId albumId)
        {
            if (AlbumIds == null)
            {
                return false;
            }

            return AlbumIds.Contains(albumId);
        }

        public void SetTotalDuration(TimeSpan totalDuration)
        {
            Guard.NotNegative(totalDuration, nameof(totalDuration));

            TotalDuration = totalDuration;
            UpdatedAt = DateTimeOffset.Now;
        }
    }
}
