using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Index
{
    /// <summary>
    /// 负责整理音乐分类
    /// </summary>
    public class MusicIndexBuilder
    {
        public IReadOnlyList<Album> BuildAlbums(IEnumerable<Music> musicList)
        {
            var albums = new Dictionary<Tuple<string, string>, Album>();

            var albumMusic = new Dictionary<Tuple<string, string>, List<Music>>();

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

                var albumTitle = NormalizeAlbumTitle(music.AlbumTitle);
                var artistName = NormalizeArtistName(music.ArtistName);

                if (string.IsNullOrWhiteSpace(albumTitle))
                {
                    albumTitle = "未知专辑";
                }

                if (string.IsNullOrWhiteSpace(artistName))
                {
                    artistName = "未知艺术家";
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
                    albumMusic[key] = new List<Music>();
                }

                if (!ContainsMusicId(album.MusicIds, music.Id))
                {
                    album.MusicIds.Add(music.Id);
                    albumMusic[key].Add(music);

                    album.TotalDuration += music.Duration;
                    album.UpdatedAt = DateTimeOffset.Now;
                }
            }

            foreach (var pair in albums)
            {
                var albumKey = pair.Key;
                Album album = pair.Value;

                var music = albumMusic[albumKey];

                album.MusicIds = CreateOrderedMusicIds(music);
                album.DiscCount = CalculateDiscCount(music);
            }

            return new List<Album>(albums.Values);
        }

        private static List<MusicId> CreateOrderedMusicIds(IEnumerable<Music> musicList)
        {
            return musicList
                .OrderBy(music =>
                    GetNumberSortKey(music.Metadata?.DiscNumber))
                .ThenBy(music =>
                    GetNumberSortKey(music.Metadata?.TrackNumber))
                .ThenBy(
                    music => NormalizeText(music.Title),
                    StringComparer.OrdinalIgnoreCase)
                .ThenBy(
                    music => music.Id.ToString(),
                    StringComparer.Ordinal)
                .Select(music => music.Id)
                .ToList();
        }

        private static long GetNumberSortKey(int? number)
        {
            if (!number.HasValue || number.Value <= 0)
            {
                return long.MaxValue;
            }

            return number.Value;
        }

        private static int CalculateDiscCount(IEnumerable<Music> musicList)
        {
            return musicList
                .Select(music => music.Metadata?.DiscNumber)
                .Where(number => number.HasValue && number.Value > 0)
                .Select(number => number.Value)
                .Distinct()
                .Count();
        }

        public IReadOnlyList<Artist> BuildArtists(IEnumerable<Music> musicList, IEnumerable<Album> albumList, ArtistGroupingMode groupingMode)
        {
            if (!Enum.IsDefined(typeof(ArtistGroupingMode), groupingMode))
            {
                throw new ArgumentOutOfRangeException(nameof(groupingMode));
            }

            var artists = new Dictionary<string, Artist>(StringComparer.Ordinal);

            // 记录每首音乐实际属于哪些艺术家，用于建立专辑关联。
            var artistsByMusicId = new Dictionary<MusicId, List<Artist>>();

            var now = DateTimeOffset.Now;

            if (musicList != null)
            {
                foreach (var music in musicList)
                {
                    if (music == null || music.Id.IsEmpty)
                    {
                        continue;
                    }

                    // 同一个 MusicId 只处理一次，防止重复累计数量和时长。
                    if (artistsByMusicId.ContainsKey(music.Id))
                    {
                        continue;
                    }

                    var musicArtists = new List<Artist>();
                    var names = GetArtistGroupingNames(music, groupingMode);

                    foreach (string name in names)
                    {
                        Artist artist;

                        if (!artists.TryGetValue(name, out artist))
                        {
                            artist = new Artist
                            {
                                Id = ArtistId.NewId(),
                                Name = name,
                                SortName = name,

                                MusicIds = new List<MusicId>(),
                                AlbumIds = new List<AlbumId>(),
                                TotalDuration = TimeSpan.Zero,

                                AddedAt = now,
                                UpdatedAt = now
                            };

                            artists.Add(name, artist);
                        }

                        artist.MusicIds.Add(music.Id);
                        artist.TotalDuration += music.Duration;

                        musicArtists.Add(artist);
                    }

                    artistsByMusicId.Add(music.Id, musicArtists);
                }
            }

            if (albumList != null)
            {
                foreach (var album in albumList)
                {
                    if (album == null || album.Id.IsEmpty || album.MusicIds == null)
                    {
                        continue;
                    }

                    foreach (var musicId in album.MusicIds)
                    {
                        List<Artist> musicArtists;

                        if (!artistsByMusicId.TryGetValue(musicId, out musicArtists))
                        {
                            continue;
                        }

                        foreach (var artist in musicArtists)
                        {
                            if (!ContainsAlbumId(artist.AlbumIds, album.Id))
                            {
                                artist.AlbumIds.Add(album.Id);
                            }
                        }
                    }
                }
            }

            return new List<Artist>(artists.Values);
        }


        private static string NormalizeText(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        internal static Tuple<string, string> CreateAlbumKey(string albumTitle, string artistName)
        {
            return Tuple.Create(NormalizeAlbumTitle(albumTitle), NormalizeArtistName(artistName));
        }

        private static string NormalizeAlbumTitle(string value)
        {
            string title = NormalizeText(value);

            return string.IsNullOrWhiteSpace(title)
                ? "未知专辑"
                : title;
        }

        internal static string NormalizeArtistName(string value)
        {
            string name = NormalizeText(value);

            return string.IsNullOrWhiteSpace(name)
                ? "未知艺术家"
                : name;
        }

        private static List<string> GetArtistGroupingNames(Music music, ArtistGroupingMode groupingMode)
        {
            var names = ArtistNameNormalizer.Normalize(
                music.Metadata?.ArtistNames);

            if (names.Count == 0)
            {
                // 兼容只有显示名称的数据，不按照分隔符猜测拆分。
                names.Add(NormalizeArtistName(music.ArtistName));
            }

            if (groupingMode == ArtistGroupingMode.Combined)
            {
                return new List<string> { string.Join("; ", names)};
            }

            return names;
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
