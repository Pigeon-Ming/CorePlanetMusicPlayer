using CorePlanetMusicPlayer.Playback.Modes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Settings
{
    public sealed class AppSettings
    {
        public int Version { get; set; }

        public PlaybackSettings Playback { get; set; }

        public LibrarySettings Library { get; set; }

        public AppearanceSettings Appearance { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public static AppSettings CreateDefault()
        {
            return new AppSettings
            {
                Version = 1,
                Playback = PlaybackSettings.CreateDefault(),
                Library = LibrarySettings.CreateDefault(),
                Appearance = AppearanceSettings.CreateDefault(),
                UpdatedAt = DateTimeOffset.Now
            };
        }

        public void Normalize()
        {
            if (Version <= 0)
            {
                Version = 1;
            }

            if (Playback == null)
            {
                Playback = PlaybackSettings.CreateDefault();
            }
            else
            {
                Playback.Normalize();
            }

            if (Library == null)
            {
                Library = LibrarySettings.CreateDefault();
            }
            else
            {
                Library.Normalize();
            }

            if (Appearance == null)
            {
                Appearance = AppearanceSettings.CreateDefault();
            }
            else
            {
                Appearance.Normalize();
            }

            if (UpdatedAt == default(DateTimeOffset))
            {
                UpdatedAt = DateTimeOffset.Now;
            }
        }
    }

    public sealed class PlaybackSettings
    {
        public double Volume { get; set; }

        public bool IsMuted { get; set; }

        public PlaybackMode PlaybackMode { get; set; }

        public bool RestoreQueueOnStartup { get; set; }

        public bool ResumePositionOnStartup { get; set; }

        public static PlaybackSettings CreateDefault()
        {
            return new PlaybackSettings
            {
                Volume = 1.0,
                IsMuted = false,
                PlaybackMode = PlaybackMode.Sequential,
                RestoreQueueOnStartup = true,
                ResumePositionOnStartup = false
            };
        }

        public void Normalize()
        {
            if (Volume < 0)
            {
                Volume = 0;
            }

            if (Volume > 1)
            {
                Volume = 1;
            }

            if (!Enum.IsDefined(typeof(PlaybackMode), PlaybackMode))
            {
                PlaybackMode = PlaybackMode.Sequential;
            }
        }
    }

    public sealed class LibrarySettings
    {
        public bool AutoRefreshOnStartup { get; set; }

        public bool IncludeSubfolders { get; set; }

        public List<string> SupportedExtensions { get; set; }

        public DateTimeOffset? LastRefreshAt { get; set; }

        public static LibrarySettings CreateDefault()
        {
            return new LibrarySettings
            {
                AutoRefreshOnStartup = false,
                IncludeSubfolders = true,
                SupportedExtensions = CreateDefaultExtensions(),
                LastRefreshAt = null
            };
        }

        public void Normalize()
        {
            if (SupportedExtensions == null)
            {
                SupportedExtensions = CreateDefaultExtensions();
            }

            if (SupportedExtensions.Count == 0)
            {
                SupportedExtensions = CreateDefaultExtensions();
            }

            for (int i = 0; i < SupportedExtensions.Count; i++)
            {
                var extension = SupportedExtensions[i];

                if (string.IsNullOrWhiteSpace(extension))
                {
                    continue;
                }

                extension = extension.Trim().ToLowerInvariant();

                if (!extension.StartsWith("."))
                {
                    extension = "." + extension;
                }

                SupportedExtensions[i] = extension;
            }
        }

        private static List<string> CreateDefaultExtensions()
        {
            return new List<string>
            {
                ".mp3",
                ".flac",
                ".wav",
                ".m4a",
                ".aac",
                ".wma",
                ".ogg"
            };
        }
    }

    public sealed class AppearanceSettings
    {
        public AppThemeKind Theme { get; set; }

        public bool UseAcrylicEffect { get; set; }

        public bool ShowNowPlayingBackground { get; set; }

        public static AppearanceSettings CreateDefault()
        {
            return new AppearanceSettings
            {
                Theme = AppThemeKind.System,
                UseAcrylicEffect = true,
                ShowNowPlayingBackground = true
            };
        }

        public void Normalize()
        {
            if (!Enum.IsDefined(typeof(AppThemeKind), Theme))
            {
                Theme = AppThemeKind.System;
            }
        }
    }

    public enum AppThemeKind
    {
        System = 0,

        Light = 1,

        Dark = 2
    }
}
