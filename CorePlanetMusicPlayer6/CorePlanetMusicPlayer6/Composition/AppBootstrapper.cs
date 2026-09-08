using CorePlanetMusicPlayer.Services.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer6.Composition
{
    public sealed class AppBootstrapper
    {
        private readonly ServiceRegistry _serviceRegistry;

        public AppBootstrapper()
        {
            _serviceRegistry = new ServiceRegistry();
        }

        public AppServices Services { get; private set;  }

        public AppSettings Settings { get; private set; }

        public async Task<AppServices> InitializeAsync()
        {
            Services = _serviceRegistry.CreateServices();

            InitializeDatabase();
            await LoadSettingsAsync();
            await ApplySettingsAsync();

            return Services;
        }

        private void InitializeDatabase()
        {
            if (Services == null || Services.Database == null)
            {
                return;
            }

            Services.Database.Initialize();
        }

        private async Task LoadSettingsAsync()
        {
            if (Services == null || Services.SettingsService == null)
            {
                Settings = AppSettings.CreateDefault();
                return;
            }

            Settings = await Services.SettingsService.LoadAsync();

            if (Settings == null)
            {
                Settings = AppSettings.CreateDefault();
            }

            Settings.Normalize();
        }

        private async Task ApplySettingsAsync()
        {
            if (Services == null || Settings == null)
            {
                return;
            }

            if (Services.PlaybackService != null && Settings.Playback != null)
            {
                await Services.PlaybackService.SetVolumeAsync(Settings.Playback.Volume);

                await Services.PlaybackService.SetPlaybackModeAsync(Settings.Playback.PlaybackMode);
            }
        }



    }
}
