using CorePlanetMusicPlayer.Data.Database;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Playback.Player;
using CorePlanetMusicPlayer.Services.Artwork;
using CorePlanetMusicPlayer.Services.History;
using CorePlanetMusicPlayer.Services.Library;
using CorePlanetMusicPlayer.Services.Lyrics;
using CorePlanetMusicPlayer.Services.Metadata;
using CorePlanetMusicPlayer.Services.Playlists;
using CorePlanetMusicPlayer.Services.Settings;
using CorePlanetMusicPlayer.Services.Statistics;
using CorePlanetMusicPlayer.Uwp.Platform.Imaging;
using CorePlanetMusicPlayer.Uwp.Platform.Metadata;
using CorePlanetMusicPlayer.Uwp.Platform.Playback;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using CorePlanetMusicPlayer.Uwp.Platform.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer6.Composition
{
    public sealed class AppServices
    {
        public LibraryDatabase Database { get; set; }

        public IMusicRepository MusicRepository { get; set; }

        public IAlbumRepository AlbumRepository { get; set; }

        public IArtistRepository ArtistRepository { get; set; }

        public IPlaylistRepository PlaylistRepository { get; set; }

        public IPlaybackHistoryRepository PlaybackHistoryRepository { get; set; }

        public ILibraryFolderRepository LibraryFolderRepository { get; set; }

        public ILyricRepository LyricRepository { get; set; }

        public IMusicLibraryService MusicLibraryService { get; set; }

        public IPlaylistService PlaylistService { get; set; }

        public ILyricService LyricService { get; set; }

        public IArtworkService ArtworkService { get; set; }

        public IPlaybackHistoryService PlaybackHistoryService { get; set; }

        public IPlaybackStatisticsService PlaybackStatisticsService { get; set; }

        public IMusicMetadataEditService MusicMetadataEditService { get; set; }

        public ISettingsService SettingsService { get; set; }

        public IPlaybackService PlaybackService { get; set; }

        public UwpFolderPickerService FolderPickerService { get; set; }

        public UwpFilePickerService FilePickerService { get; set; }

        public UwpStorageFileMapper StorageFileMapper { get; set; }

        public UwpStorageAccessService StorageAccessService { get; set; }

        public UwpMediaSourceFactory MediaSourceFactory { get; set; }

        public UwpAudioPlayer AudioPlayer { get; set; }

        public UwpSystemMediaControlsService SystemMediaControlsService { get; set; }

        public UwpThumbnailLoader ThumbnailLoader { get; set; }

        public UwpArtworkLoader ArtworkLoader { get; set; }

        public UwpMusicMetadataWriter MusicMetadataWriter { get; set; }

        public UwpDispatcherService DispatcherService { get; set; }

        public UwpDeviceInfoService DeviceInfoService { get; set; }

        public UwpSettingsStore SettingsStore { get; set; }

        public LyricSearchService LyricSearchService { get; set; }
    }
}
