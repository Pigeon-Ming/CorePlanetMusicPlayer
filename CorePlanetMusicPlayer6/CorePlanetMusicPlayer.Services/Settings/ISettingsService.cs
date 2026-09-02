using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Settings
{
    public interface ISettingsService
    {
        Task<AppSettings> LoadAsync();

        Task SaveAsync(AppSettings settings);

        Task<AppSettings> ResetAsync();

        AppSettings GetDefault();
    }
}
