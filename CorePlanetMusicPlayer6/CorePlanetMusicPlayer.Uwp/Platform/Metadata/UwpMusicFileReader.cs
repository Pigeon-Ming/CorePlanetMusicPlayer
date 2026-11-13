using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Services.Metadata;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Uwp.Platform.Metadata
{
    public sealed class UwpMusicFileReader
    {
        private readonly UwpStorageFileMapper _mapper;
        private readonly IUwpMusicTagAccessor _musicTagAccessor;

        public UwpMusicFileReader(
            UwpStorageFileMapper mapper,
            IUwpMusicTagAccessor musicTagAccessor)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            if (musicTagAccessor == null)
            {
                throw new ArgumentNullException(
                    nameof(musicTagAccessor));
            }

            _mapper = mapper;
            _musicTagAccessor = musicTagAccessor;
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

            if (basicProperties.Size == 0)
            {
                throw new InvalidDataException(
                    "音乐文件为空。");
            }

            var fileInfo = _mapper.ToMusicFileInfo(file, libraryFolder, relativePath, basicProperties);

            MusicTagReadResult tagResult =
                await _musicTagAccessor.ReadAsync(file);

            if (tagResult == null)
            {
                throw new InvalidOperationException(
                    "歌曲标签访问器没有返回读取结果。");
            }

            MusicMetadata metadata = tagResult.Metadata;

            string title = NormalizeText(metadata.Title);

            if (string.IsNullOrWhiteSpace(title))
            {
                title = Path.GetFileNameWithoutExtension(file.Name);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = file.Name;
            }

            List<string> artistNames =
                ArtistNameNormalizer.Normalize(
                    metadata.ArtistNames ?? new List<string>());

            string artistName = string.Join("; ", artistNames);
            string albumTitle = NormalizeText(metadata.AlbumTitle);

            var music = Music.CreateLocal(
                title,
                artistName,
                albumTitle,
                tagResult.Duration,
                fileInfo);

            metadata.Title = music.Title;
            metadata.ArtistName = music.ArtistName;
            metadata.ArtistNames = artistNames;
            metadata.AlbumTitle = music.AlbumTitle;
            metadata.AlbumArtistName =
                NormalizeText(metadata.AlbumArtistName);
            metadata.Genre = NormalizeText(metadata.Genre);
            metadata.Composer = NormalizeText(metadata.Composer);
            metadata.Comment = NormalizeText(metadata.Comment);

            music.Metadata = metadata;

            return music;
        }

        private static string NormalizeText(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }
}
