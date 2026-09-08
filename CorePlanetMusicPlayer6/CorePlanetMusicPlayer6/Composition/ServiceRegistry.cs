using CorePlanetMusicPlayer.Data.Database;
using CorePlanetMusicPlayer.Data.Repositories.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using CorePlanetMusicPlayer.Uwp.Platform.Imaging;
using CorePlanetMusicPlayer.Uwp.Platform.System;
using CorePlanetMusicPlayer.Uwp.Platform.Metadata;
using CorePlanetMusicPlayer.Uwp.Platform.Playback;
using CorePlanetMusicPlayer.Playback.Queue;
using CorePlanetMusicPlayer.Playback.Modes;
using CorePlanetMusicPlayer.Playback.Player;
using CorePlanetMusicPlayer.Services.Lyrics;
using CorePlanetMusicPlayer.Services.Library;
using CorePlanetMusicPlayer.Services.Playlists;
using CorePlanetMusicPlayer.Services.Artwork;
using CorePlanetMusicPlayer.Services.Statistics;
using CorePlanetMusicPlayer.Services.History;
using CorePlanetMusicPlayer.Services.Metadata;
using CorePlanetMusicPlayer.Services.Settings;

namespace CorePlanetMusicPlayer6.Composition
{
    public sealed class ServiceRegistry
    {
        public AppServices CreateServices()
        {
            var services = new AppServices();

            CreateDatabaseServices(services);
            CreateRepositoryServices(services);
            CreatePlatformServices(services);
            CreatePlaybackServices(services);
            CreateBusinessServices(services);
            ConnectSystemMediaControls(services);

            return services;
        }

        private static void CreateDatabaseServices(AppServices services)
        {
            var options = new LibraryDatabaseOptions
            {
                DatabaseDirectory = ApplicationData.Current.LocalFolder.Path,
                DatabaseFileName = "library.db",
                TargetVersion = 1
            };

            var connectionFactory = new DatabaseConnectionFactory(options);

            var migrator = new DatabaseMigrator();

            services.Database = new LibraryDatabase(
                options,
                connectionFactory,
                migrator);
        }

        private static void CreateRepositoryServices(AppServices services)
        {
            services.MusicRepository = new SqliteMusicRepository(services.Database);

            services.AlbumRepository = new SqliteAlbumRepository(services.Database);

            services.ArtistRepository = new SqliteArtistRepository(services.Database);

            services.PlaylistRepository = new SqlitePlaylistRepository(services.Database);

            services.PlaybackHistoryRepository = new SqlitePlaybackHistoryRepository(services.Database);

            services.LibraryFolderRepository = new SqliteLibraryFolderRepository(services.Database);

            services.LyricRepository = new SqliteLyricRepository(services.Database);
        }

        private static void CreatePlatformServices(AppServices services)
        {
            services.FolderPickerService = new UwpFolderPickerService();

            services.FilePickerService = new UwpFilePickerService();

            services.StorageFileMapper = new UwpStorageFileMapper();
            
            services.StorageAccessService = new UwpStorageAccessService(services.FolderPickerService, services.StorageFileMapper);

            services.ThumbnailLoader = new UwpThumbnailLoader();

            services.ArtworkLoader = new UwpArtworkLoader(services.LibraryFolderRepository, services.StorageAccessService, services.ThumbnailLoader);

            services.DispatcherService = new UwpDispatcherService();

            services.DeviceInfoService = new UwpDeviceInfoService();

            services.SettingsStore = new UwpSettingsStore();

            services.MusicMetadataWriter = new UwpMusicMetadataWriter(services.LibraryFolderRepository, services.StorageAccessService);
        }

        private static void CreatePlaybackServices(AppServices services)
        {
            services.MediaSourceFactory = new UwpMediaSourceFactory(services.MusicRepository, services.LibraryFolderRepository, services.StorageAccessService);

            services.AudioPlayer = new UwpAudioPlayer(services.MediaSourceFactory);

            var queue = new PlaybackQueue();

            var strategies = new IPlaybackModeStrategy[]
            {
                new SequentialPlaybackModeStrategy(),
                new RepeatAllPlaybackModeStrategy(),
                new RepeatOnePlaybackModeStrategy(),
                new ShufflePlaybackModeStrategy(),
                new ReversePlaybackModeStrategy()
            };

            services.PlaybackService = new PlaybackService(services.AudioPlayer, queue, strategies);
        }

        private static void CreateBusinessServices(AppServices services)
        {
            var lyricParserCollection = new LyricParserCollection(new ILyricParser[] {new LrcParser()});

            var libraryQueryService = new LibraryQueryService(services.MusicRepository, services.AlbumRepository, services.ArtistRepository, services.LibraryFolderRepository);

            services.MusicLibraryService = new MusicLibraryService(services.MusicRepository, services.AlbumRepository, services.ArtistRepository, services.LibraryFolderRepository, new LibraryScanner(), new MusicIndexBuilder(), libraryQueryService);

            services.PlaylistService = new PlaylistService(services.PlaylistRepository);

            services.LyricSearchService = new LyricSearchService();

            services.LyricService = new LyricService(services.LyricRepository, lyricParserCollection, services.LyricSearchService);

            services.ArtworkService = new ArtworkService(services.MusicRepository);

            services.PlaybackHistoryService = new PlaybackHistoryService(services.PlaybackHistoryRepository);

            services.PlaybackStatisticsService = new PlaybackStatisticsService(services.PlaybackHistoryRepository, services.MusicRepository);

            services.MusicMetadataEditService = new MusicMetadataEditService(services.MusicRepository, services.MusicMetadataWriter);

            services.SettingsService = new SettingsService(services.SettingsStore);
        }

        private static void ConnectSystemMediaControls(AppServices services)
        {
            services.SystemMediaControlsService = new UwpSystemMediaControlsService(services.AudioPlayer.NativeMediaPlayer);

            services.SystemMediaControlsService.PlayRequested += async (sender, args) =>
            {
                await services.PlaybackService.ResumeAsync();
            };

            services.SystemMediaControlsService.PauseRequested += async (sender, args) =>
            {
                await services.PlaybackService.PauseAsync();
            };

            services.SystemMediaControlsService.NextRequested += async (sender, args) =>
            {
                await services.PlaybackService.NextAsync();
            };

            services.SystemMediaControlsService.PreviousRequested += async (sender, args) =>
            {
                await services.PlaybackService.PreviousAsync();
            };

            services.SystemMediaControlsService.StopRequested += async (sender, args) =>
            {
                await services.PlaybackService.StopAsync();
            };
        }

        private static string CreateDatabasePath()
        {
            return System.IO.Path.Combine(
                ApplicationData.Current.LocalFolder.Path,
                "library.db");
        }
    }
}
