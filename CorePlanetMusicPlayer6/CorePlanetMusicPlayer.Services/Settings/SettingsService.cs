using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Settings
{
    public sealed class SettingsService : ISettingsService
    {
        private readonly ISettingsStore _settingsStore;

        public SettingsService(ISettingsStore settingsStore)
        {
            _settingsStore = settingsStore;
        }

        public async Task<AppSettings> LoadAsync()
        {
            AppSettings settings = null;

            if (_settingsStore != null)
            {
                settings = await _settingsStore.LoadAsync();
            }

            if (settings == null)
            {
                settings = AppSettings.CreateDefault();
            }

            settings.Normalize();

            return settings;
        }

        public async Task SaveAsync(AppSettings settings)
        {
            Guard.NotNull(settings, nameof(settings));

            settings.Normalize();
            settings.UpdatedAt = DateTimeOffset.Now;

            if (_settingsStore != null)
            {
                await _settingsStore.SaveAsync(settings);
            }
        }

        public async Task<AppSettings> ResetAsync()
        {
            var settings = AppSettings.CreateDefault();

            if (_settingsStore != null)
            {
                await _settingsStore.ClearAsync();
                await _settingsStore.SaveAsync(settings);
            }

            return settings;
        }

        public AppSettings GetDefault()
        {
            return AppSettings.CreateDefault();
        }
    }
}
