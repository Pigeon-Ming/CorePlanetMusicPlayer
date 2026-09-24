using CorePlanetMusicPlayer.Data.Database;
using CorePlanetMusicPlayer.Data.Repositories.Sqlite;
using CorePlanetMusicPlayer.Playback.Modes;
using CorePlanetMusicPlayer.Playback.Player;
using CorePlanetMusicPlayer.Playback.Queue;
using CorePlanetMusicPlayer.Services.Artwork;
using CorePlanetMusicPlayer.Services.History;
using CorePlanetMusicPlayer.Services.Library;
using CorePlanetMusicPlayer.Services.Library.Albums;
using CorePlanetMusicPlayer.Services.Library.Artists;
using CorePlanetMusicPlayer.Services.Library.Genres;
using CorePlanetMusicPlayer.Services.Library.Index;
using CorePlanetMusicPlayer.Services.Library.MusicQueries;
using CorePlanetMusicPlayer.Services.Library.Years;
using CorePlanetMusicPlayer.Services.Lyrics;
using CorePlanetMusicPlayer.Services.Metadata;
using CorePlanetMusicPlayer.Services.Playback;
using CorePlanetMusicPlayer.Services.Playlists;
using CorePlanetMusicPlayer.Services.Settings;
using CorePlanetMusicPlayer.Services.Statistics;
using CorePlanetMusicPlayer.Uwp.Platform.Imaging;
using CorePlanetMusicPlayer.Uwp.Platform.Library;
using CorePlanetMusicPlayer.Uwp.Platform.Metadata;
using CorePlanetMusicPlayer.Uwp.Platform.Playback;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using CorePlanetMusicPlayer.Uwp.Platform.System;
using Windows.Storage;

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

            var musicResolver = new PlaybackMusicResolver(services.MusicRepository);

            services.PlaybackService = new PlaybackService(services.AudioPlayer, queue, strategies, musicResolver);
        }

        private static void CreateBusinessServices(AppServices services)
        {
            var lyricParserCollection = new LyricParserCollection(new ILyricParser[] {new LrcParser()});

            var musicFileReader = new UwpMusicFileReader(services.StorageFileMapper);

            var libraryScanner = new UwpLibraryScanner(services.StorageAccessService, services.StorageFileMapper, musicFileReader);

            services.SettingsService = new SettingsService(services.SettingsStore);

            var libraryWriteCoordinator = new LibraryWriteCoordinator();

            var musicIndexService = new MusicIndexService(services.MusicRepository, services.AlbumRepository, services.ArtistRepository, new MusicIndexBuilder(), libraryWriteCoordinator, services.SettingsService);

            services.MusicIndexService = musicIndexService;

            services.MusicQueryService = new MusicQueryService(services.MusicRepository);

            services.GenreQueryService = new GenreQueryService(services.MusicRepository);

            services.YearQueryService = new YearQueryService(services.MusicRepository);

            services.AlbumService = new AlbumService(services.AlbumRepository, services.MusicQueryService, libraryWriteCoordinator);

            services.ArtistService = new ArtistService(services.ArtistRepository, services.AlbumRepository, services.MusicQueryService, libraryWriteCoordinator);

            services.MusicLibraryService = new MusicLibraryService(services.MusicRepository, services.LibraryFolderRepository, libraryScanner, musicIndexService);

            services.PlaylistService = new PlaylistService(services.PlaylistRepository, services.MusicRepository);

            services.LyricSearchService = new LyricSearchService();

            services.LyricService = new LyricService(services.LyricRepository, lyricParserCollection, services.LyricSearchService);

            services.ArtworkService = new ArtworkService(services.MusicRepository);

            services.PlaybackHistoryService = new PlaybackHistoryService(services.PlaybackHistoryRepository);

            services.PlaybackHistoryRecorder = new PlaybackHistoryRecorder(services.PlaybackService, services.PlaybackHistoryService);

            services.PlaybackStatisticsService = new PlaybackStatisticsService(services.PlaybackHistoryRepository, services.MusicRepository);

            services.MusicMetadataEditService = new MusicMetadataEditService(services.MusicRepository, services.MusicMetadataWriter, musicIndexService);

            services.PlaybackSessionService = new PlaybackSessionService(services.PlaybackService, services.SettingsService, new UwpPlaybackSessionStore());
        }

        private static void ConnectSystemMediaControls(AppServices services)
        {
            services.SystemMediaControlsService = new UwpSystemMediaControlsService(services.AudioPlayer.NativeMediaPlayer);

            services.SystemMediaControlsService.PlayRequested += async (sender, args) =>
            {
                await services.PlaybackService.StartOrResumeAsync();
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
