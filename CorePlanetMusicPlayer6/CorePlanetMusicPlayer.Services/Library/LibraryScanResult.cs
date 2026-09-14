using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library
{
    /// <summary>
    /// 用于表示音乐库扫描结果的类
    /// </summary>

    public sealed class LibraryScanResult
    {
        public IReadOnlyList<Music> Items { get; }

        public bool IsComplete { get; }

        public IReadOnlyList<string> Errors { get; }

        private LibraryScanResult(IEnumerable<Music> items, bool isComplete, IEnumerable<string> errors)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            Items = items.ToList().AsReadOnly();
            IsComplete = isComplete;
            Errors = errors.ToList().AsReadOnly();
        }

        public static LibraryScanResult Completed(IEnumerable<Music> items)
        {
            return new LibraryScanResult(items, true, new List<string>());
        }

        public static LibraryScanResult Incomplete(IEnumerable<Music> items, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new ArgumentException("An incomplete scan must include an error message.", nameof(errorMessage));
            }

            return new LibraryScanResult(items, false, new[] { errorMessage });
        }
    }
}
