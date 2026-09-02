using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Settings
{
    public interface ISettingsStore
    {
        Task<AppSettings> LoadAsync();

        Task SaveAsync(AppSettings settings);

        Task ClearAsync();
    }
}
