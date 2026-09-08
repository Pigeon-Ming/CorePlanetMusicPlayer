using CorePlanetMusicPlayer.Playback.Modes;
using CorePlanetMusicPlayer.Services.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Uwp.Platform.System
{
    public sealed class UwpSettingsStore: ISettingsStore
    {
        private const string VersionKey = "Settings.Version";
        private const string UpdatedAtKey = "Settings.UpdatedAt";

        private const string PlaybackVolumeKey = "Settings.Playback.Volume";
        private const string PlaybackIsMutedKey = "Settings.Playback.IsMuted";
        private const string PlaybackModeKey = "Settings.Playback.PlaybackMode";
        private const string PlaybackRestoreQueueOnStartupKey = "Settings.Playback.RestoreQueueOnStartup";
        private const string PlaybackResumePositionOnStartupKey = "Settings.Playback.ResumePositionOnStartup";

        private const string LibraryAutoRefreshOnStartupKey = "Settings.Library.AutoRefreshOnStartup";
        private const string LibraryIncludeSubfoldersKey = "Settings.Library.IncludeSubfolders";
        private const string LibrarySupportedExtensionsKey = "Settings.Library.SupportedExtensions";
        private const string LibraryLastRefreshAtKey = "Settings.Library.LastRefreshAt";

        private const string AppearanceThemeKey = "Settings.Appearance.Theme";
        private const string AppearanceUseAcrylicEffectKey = "Settings.Appearance.UseAcrylicEffect";
        private const string AppearanceShowNowPlayingBackgroundKey = "Settings.Appearance.ShowNowPlayingBackground";

        public Task<AppSettings> LoadAsync()
        {
            var values = ApplicationData.Current.LocalSettings.Values;

            if (!values.ContainsKey(VersionKey))
            {
                return Task.FromResult<AppSettings>(null);
            }

            var settings = AppSettings.CreateDefault();

            settings.Version = GetInt32(values, VersionKey, settings.Version);
            settings.UpdatedAt = GetDateTimeOffset(
                values,
                UpdatedAtKey,
                settings.UpdatedAt);

            settings.Playback.Volume = GetDouble(
                values,
                PlaybackVolumeKey,
                settings.Playback.Volume);

            settings.Playback.IsMuted = GetBoolean(
                values,
                PlaybackIsMutedKey,
                settings.Playback.IsMuted);

            settings.Playback.PlaybackMode = (PlaybackMode)GetInt32(
                values,
                PlaybackModeKey,
                (int)settings.Playback.PlaybackMode);

            settings.Playback.RestoreQueueOnStartup = GetBoolean(
                values,
                PlaybackRestoreQueueOnStartupKey,
                settings.Playback.RestoreQueueOnStartup);

            settings.Playback.ResumePositionOnStartup = GetBoolean(
                values,
                PlaybackResumePositionOnStartupKey,
                settings.Playback.ResumePositionOnStartup);

            settings.Library.AutoRefreshOnStartup = GetBoolean(
                values,
                LibraryAutoRefreshOnStartupKey,
                settings.Library.AutoRefreshOnStartup);

            settings.Library.IncludeSubfolders = GetBoolean(
                values,
                LibraryIncludeSubfoldersKey,
                settings.Library.IncludeSubfolders);

            settings.Library.SupportedExtensions = GetStringList(
                values,
                LibrarySupportedExtensionsKey,
                settings.Library.SupportedExtensions);

            settings.Library.LastRefreshAt = GetNullableDateTimeOffset(
                values,
                LibraryLastRefreshAtKey);

            settings.Appearance.Theme = (AppThemeKind)GetInt32(
                values,
                AppearanceThemeKey,
                (int)settings.Appearance.Theme);

            settings.Appearance.UseAcrylicEffect = GetBoolean(
                values,
                AppearanceUseAcrylicEffectKey,
                settings.Appearance.UseAcrylicEffect);

            settings.Appearance.ShowNowPlayingBackground = GetBoolean(
                values,
                AppearanceShowNowPlayingBackgroundKey,
                settings.Appearance.ShowNowPlayingBackground);

            settings.Normalize();

            return Task.FromResult(settings);
        }

        public Task SaveAsync(AppSettings settings)
        {
            if (settings == null)
            {
                return Task.FromResult<object>(null);
            }

            settings.Normalize();
            settings.UpdatedAt = DateTimeOffset.Now;

            var values = ApplicationData.Current.LocalSettings.Values;

            values[VersionKey] = settings.Version;
            values[UpdatedAtKey] = ToUnixTimeMilliseconds(settings.UpdatedAt);

            values[PlaybackVolumeKey] = settings.Playback.Volume;
            values[PlaybackIsMutedKey] = settings.Playback.IsMuted;
            values[PlaybackModeKey] = (int)settings.Playback.PlaybackMode;
            values[PlaybackRestoreQueueOnStartupKey] = settings.Playback.RestoreQueueOnStartup;
            values[PlaybackResumePositionOnStartupKey] = settings.Playback.ResumePositionOnStartup;

            values[LibraryAutoRefreshOnStartupKey] = settings.Library.AutoRefreshOnStartup;
            values[LibraryIncludeSubfoldersKey] = settings.Library.IncludeSubfolders;
            values[LibrarySupportedExtensionsKey] = JoinStringList(settings.Library.SupportedExtensions);

            if (settings.Library.LastRefreshAt.HasValue)
            {
                values[LibraryLastRefreshAtKey] =
                    ToUnixTimeMilliseconds(settings.Library.LastRefreshAt.Value);
            }
            else
            {
                Remove(values, LibraryLastRefreshAtKey);
            }

            values[AppearanceThemeKey] = (int)settings.Appearance.Theme;
            values[AppearanceUseAcrylicEffectKey] = settings.Appearance.UseAcrylicEffect;
            values[AppearanceShowNowPlayingBackgroundKey] = settings.Appearance.ShowNowPlayingBackground;

            return Task.FromResult<object>(null);
        }

        public Task ClearAsync()
        {
            var values = ApplicationData.Current.LocalSettings.Values;

            Remove(values, VersionKey);
            Remove(values, UpdatedAtKey);

            Remove(values, PlaybackVolumeKey);
            Remove(values, PlaybackIsMutedKey);
            Remove(values, PlaybackModeKey);
            Remove(values, PlaybackRestoreQueueOnStartupKey);
            Remove(values, PlaybackResumePositionOnStartupKey);

            Remove(values, LibraryAutoRefreshOnStartupKey);
            Remove(values, LibraryIncludeSubfoldersKey);
            Remove(values, LibrarySupportedExtensionsKey);
            Remove(values, LibraryLastRefreshAtKey);

            Remove(values, AppearanceThemeKey);
            Remove(values, AppearanceUseAcrylicEffectKey);
            Remove(values, AppearanceShowNowPlayingBackgroundKey);

            return Task.FromResult<object>(null);
        }

        private static int GetInt32(
            Windows.Foundation.Collections.IPropertySet values,
            string key,
            int defaultValue)
        {
            object value;

            if (!values.TryGetValue(key, out value) || value == null)
            {
                return defaultValue;
            }

            if (value is int)
            {
                return (int)value;
            }

            int result;

            if (int.TryParse(value.ToString(), out result))
            {
                return result;
            }

            return defaultValue;
        }

        private static double GetDouble(
            Windows.Foundation.Collections.IPropertySet values,
            string key,
            double defaultValue)
        {
            object value;

            if (!values.TryGetValue(key, out value) || value == null)
            {
                return defaultValue;
            }

            if (value is double)
            {
                return (double)value;
            }

            double result;

            if (double.TryParse(value.ToString(), out result))
            {
                return result;
            }

            return defaultValue;
        }

        private static bool GetBoolean(
            Windows.Foundation.Collections.IPropertySet values,
            string key,
            bool defaultValue)
        {
            object value;

            if (!values.TryGetValue(key, out value) || value == null)
            {
                return defaultValue;
            }

            if (value is bool)
            {
                return (bool)value;
            }

            bool result;

            if (bool.TryParse(value.ToString(), out result))
            {
                return result;
            }

            return defaultValue;
        }

        private static DateTimeOffset GetDateTimeOffset(
            Windows.Foundation.Collections.IPropertySet values,
            string key,
            DateTimeOffset defaultValue)
        {
            var nullableValue = GetNullableDateTimeOffset(values, key);

            if (nullableValue.HasValue)
            {
                return nullableValue.Value;
            }

            return defaultValue;
        }

        private static DateTimeOffset? GetNullableDateTimeOffset(
            Windows.Foundation.Collections.IPropertySet values,
            string key)
        {
            object value;

            if (!values.TryGetValue(key, out value) || value == null)
            {
                return null;
            }

            long milliseconds;

            if (value is long)
            {
                milliseconds = (long)value;
                return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            }

            if (long.TryParse(value.ToString(), out milliseconds))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            }

            return null;
        }

        private static List<string> GetStringList(
            Windows.Foundation.Collections.IPropertySet values,
            string key,
            List<string> defaultValue)
        {
            object value;

            if (!values.TryGetValue(key, out value) || value == null)
            {
                return defaultValue == null
                    ? new List<string>()
                    : new List<string>(defaultValue);
            }

            var text = value.ToString();

            if (string.IsNullOrWhiteSpace(text))
            {
                return defaultValue == null
                    ? new List<string>()
                    : new List<string>(defaultValue);
            }

            var parts = text.Split(
                new[] { '|' },
                StringSplitOptions.RemoveEmptyEntries);

            var result = new List<string>();

            for (int i = 0; i < parts.Length; i++)
            {
                var item = parts[i];

                if (!string.IsNullOrWhiteSpace(item))
                {
                    result.Add(item.Trim());
                }
            }

            return result;
        }

        private static string JoinStringList(List<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return string.Empty;
            }

            return string.Join("|", values);
        }

        private static long ToUnixTimeMilliseconds(DateTimeOffset value)
        {
            return value.ToUnixTimeMilliseconds();
        }

        private static void Remove(
            Windows.Foundation.Collections.IPropertySet values,
            string key)
        {
            if (values.ContainsKey(key))
            {
                values.Remove(key);
            }
        }
    }
}
