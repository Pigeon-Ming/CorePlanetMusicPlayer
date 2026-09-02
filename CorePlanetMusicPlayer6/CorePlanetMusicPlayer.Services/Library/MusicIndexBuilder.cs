using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library
{
    /// <summary>
    /// 负责整理音乐分类
    /// </summary>
    public class MusicIndexBuilder
    {
        public IReadOnlyList<Album> BuildAlbums(IEnumerable<Music> musicList)
        {
            var albums = new Dictionary<string, Album>();

            if (musicList == null)
            {
                return new List<Album>();
            }

            foreach (var music in musicList)
            {
                if (music == null || music.Id.IsEmpty)
                {
                    continue;
                }

                var albumTitle = NormalizeText(music.AlbumTitle);
                var artistName = NormalizeText(music.ArtistName);

                if (string.IsNullOrWhiteSpace(albumTitle))
                {
                    albumTitle = "未知专辑";
                }

                if (string.IsNullOrWhiteSpace(artistName))
                {
                    artistName = "未知专辑";
                }

                var key = CreateAlbumKey(albumTitle, artistName);

                Album album;

                if (!albums.TryGetValue(key, out album))
                {
                    album = new Album
                    {
                        Id = AlbumId.NewId(),
                        Title = albumTitle,
                        ArtistName = artistName,
                        AlbumArtistName = GetAlbumArtistName(music),
                        Genre = GetGenre(music),
                        Year = GetYear(music),
                        MusicIds = new List<MusicId>(),
                        TotalDuration = TimeSpan.Zero,
                        AddedAt = DateTimeOffset.Now,
                        UpdatedAt = DateTimeOffset.Now
                    };

                    albums[key] = album;
                }

                if (!ContainsMusicId(album.MusicIds, music.Id))
                {
                    album.MusicIds.Add(music.Id);
                    album.TotalDuration = album.TotalDuration + music.Duration;
                    album.UpdatedAt = DateTimeOffset.Now;
                }
            }

            return new List<Album>(albums.Values);
        }

        public IReadOnlyList<Artist> BuildArtists(IEnumerable<Music> musicList, IEnumerable<Album> albumList)
        {
            var artists = new Dictionary<string, Artist>();

            if (musicList != null)
            {
                foreach (var music in musicList)
                {
                    if (music == null || music.Id.IsEmpty)
                    {
                        continue;
                    }

                    var artistName = NormalizeText(music.ArtistName);

                    if (string.IsNullOrWhiteSpace(artistName))
                    {
                        artistName = "未知艺术家";
                    }

                    Artist artist;

                    if (!artists.TryGetValue(artistName, out artist))
                    {
                        artist = new Artist
                        {
                            Id = ArtistId.NewId(),
                            Name = artistName,
                            SortName = artistName,
                            MusicIds = new List<MusicId>(),
                            AlbumIds = new List<AlbumId>(),
                            TotalDuration = TimeSpan.Zero,
                            AddedAt = DateTimeOffset.Now,
                            UpdatedAt = DateTimeOffset.Now
                        };

                        artists[artistName] = artist;
                    }

                    if (!ContainsMusicId(artist.MusicIds, music.Id))
                    {
                        artist.MusicIds.Add(music.Id);
                        artist.TotalDuration += music.Duration;
                        artist.UpdatedAt = DateTimeOffset.Now;
                    }
                }
            }

            if (albumList != null)
            {
                foreach (var album in albumList)
                {
                    if (album == null || album.Id.IsEmpty)
                    {
                        continue;
                    }

                    var artistName = NormalizeText(album.ArtistName);

                    if (string.IsNullOrWhiteSpace(artistName))
                    {
                        artistName = "未知艺术家";
                    }

                    Artist artist;

                    if (!artists.TryGetValue(artistName, out artist))
                    {
                        artist = new Artist
                        {
                            Id = ArtistId.NewId(),
                            Name = artistName,
                            SortName = artistName,
                            MusicIds = new List<MusicId>(),
                            AlbumIds = new List<AlbumId>(),
                            TotalDuration = TimeSpan.Zero,
                            AddedAt = DateTimeOffset.Now,
                            UpdatedAt = DateTimeOffset.Now
                        };

                        artists[artistName] = artist;
                    }

                    if (!ContainsAlbumId(artist.AlbumIds, album.Id))
                    {
                        artist.AlbumIds.Add(album.Id);
                        artist.UpdatedAt = DateTimeOffset.Now;
                    }
                }
            }

            return new List<Artist>(artists.Values);
        }


        private static string NormalizeText(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        private string CreateAlbumKey(string albumTitle, string artistName)
        {
            return albumTitle + "|" + artistName;
        }

        private string GetAlbumArtistName(Music music)
        {
            if (music.Metadata == null)
            {
                return string.Empty;
            }

            return NormalizeText(music.Metadata.AlbumArtistName);
        }

        private string GetGenre(Music music)
        {
            if (music.Metadata == null)
            {
                return string.Empty;
            }

            return NormalizeText(music.Metadata.Genre);
        }

        private int? GetYear(Music music)
        {
            if (music.Metadata == null)
            {
                return null;
            }

            return music.Metadata.Year;
        }

        private bool ContainsMusicId(List<MusicId> musicIds, MusicId musicId)
        {
            if (musicIds == null)
            {
                return false;
            }

            for (int i = 0; i < musicIds.Count; i++)
            {
                if (musicIds[i] == musicId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsAlbumId(IList<AlbumId> albumIds, AlbumId albumId)
        {
            if (albumIds == null)
            {
                return false;
            }

            for (int i = 0; i < albumIds.Count; i++)
            {
                if (albumIds[i] == albumId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
