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

            string artistName = NormalizeText(properties.Artist);
            string albumTitle = NormalizeText(properties.Album);

            var music = Music.CreateLocal(title, artistName, albumTitle, properties.Duration, fileInfo);

            music.Metadata = new MusicMetadata
            {
                Title = music.Title,
                ArtistName = music.ArtistName,
                AlbumTitle = music.AlbumTitle,
                AlbumArtistName = NormalizeText(properties.AlbumArtist),
                Genre = GetFirstValue(properties.Genre),
                Year = ToNullableInt(properties.Year),
                TrackNumber = ToNullableInt(properties.TrackNumber),
                Composer = JoinValues(properties.Composers)
            };

            return music;
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
