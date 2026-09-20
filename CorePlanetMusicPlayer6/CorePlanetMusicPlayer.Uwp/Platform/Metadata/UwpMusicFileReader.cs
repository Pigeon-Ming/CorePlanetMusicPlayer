using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace CorePlanetMusicPlayer.Uwp.Platform.Metadata
{
    public sealed class UwpMusicFileReader
    {
        private readonly UwpStorageFileMapper _mapper;

        public UwpMusicFileReader(UwpStorageFileMapper mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            _mapper = mapper;
        }

        public async Task<Music> ReadAsync(StorageFile file, LibraryFolder libraryFolder, string relativePath)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (libraryFolder == null)
            {
                throw new ArgumentNullException(nameof(libraryFolder));
            }

            if (!_mapper.IsSupportedMusicFile(file))
            {
                throw new NotSupportedException(
                    $"不支持的音乐文件扩展名：{file.FileType}");
            }

            var basicProperties = await file.GetBasicPropertiesAsync();

            var fileInfo = _mapper.ToMusicFileInfo(file, libraryFolder, relativePath, basicProperties);

            var properties = await file.Properties.GetMusicPropertiesAsync();

            string title = NormalizeText(properties.Title);

            if (string.IsNullOrWhiteSpace(title))
            {
                title = Path.GetFileNameWithoutExtension(file.Name);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = file.Name;
            }

            var artistNames = await ReadArtistNamesAsync(properties);

            string artistName = string.Join("; ", artistNames);
            string albumTitle = NormalizeText(properties.Album);

            var music = Music.CreateLocal(title, artistName, albumTitle, properties.Duration, fileInfo);

            music.Metadata = new MusicMetadata
            {
                Title = music.Title,
                ArtistName = music.ArtistName,
                ArtistNames = artistNames,
                AlbumTitle = music.AlbumTitle,
                AlbumArtistName = NormalizeText(properties.AlbumArtist),
                Genre = GetFirstValue(properties.Genre),
                Year = ToNullableInt(properties.Year),
                TrackNumber = ToNullableInt(properties.TrackNumber),
                Composer = JoinValues(properties.Composers)
            };

            return music;
        }

        private static async Task<List<string>> ReadArtistNamesAsync(MusicProperties properties)
        {
            const string propertyName = "System.Music.Artist";

            var values = await properties.RetrievePropertiesAsync(new[] { propertyName });

            object rawValue;

            if (!values.TryGetValue(propertyName, out rawValue) || rawValue == null)
            {
                return new List<string>();
            }

            var names = rawValue as IEnumerable<string>;

            if (names != null)
            {
                return ArtistNameNormalizer.Normalize(names);
            }

            // 如果返回单个字符串，将其作为一个完整名称。
            var singleName = rawValue as string;

            if (singleName != null)
            {
                return ArtistNameNormalizer.Normalize(new[] { singleName });
            }

            throw new InvalidOperationException("艺术家属性的数据类型不受支持。");
        }

        private static string NormalizeText(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        private static int? ToNullableInt(uint value)
        {
            if (value == 0 || value > int.MaxValue)
            {
                return null;
            }

            return (int)value;
        }

        private static string GetFirstValue(IEnumerable<string> values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            return values.Select(NormalizeText).FirstOrDefault(value => value.Length > 0) ?? string.Empty;
        }

        private static string JoinValues(IEnumerable<string> values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            return string.Join("; ",values.Select(NormalizeText).Where(value => value.Length > 0));
        }
    }
}
