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

        public IReadOnlyList<string> Warnings { get; }

        private LibraryScanResult(
            IEnumerable<Music> items,
            bool isComplete,
            IEnumerable<string> errors,
            IEnumerable<string> warnings)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            if (warnings == null)
            {
                throw new ArgumentNullException(nameof(warnings));
            }

            Items = items.ToList().AsReadOnly();
            IsComplete = isComplete;
            Errors = errors.ToList().AsReadOnly();
            Warnings = warnings.ToList().AsReadOnly();
        }

        public static LibraryScanResult Completed(IEnumerable<Music> items)
        {
            return Completed(items, new List<string>());
        }

        public static LibraryScanResult Completed(
            IEnumerable<Music> items,
            IEnumerable<string> warnings)
        {
            return new LibraryScanResult(
                items,
                true,
                new List<string>(),
                warnings);
        }

        public static LibraryScanResult Incomplete(IEnumerable<Music> items, string errorMessage)
        {
            return Incomplete(
                items,
                errorMessage,
                new List<string>());
        }

        public static LibraryScanResult Incomplete(
            IEnumerable<Music> items,
            string errorMessage,
            IEnumerable<string> warnings)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new ArgumentException("An incomplete scan must include an error message.", nameof(errorMessage));
            }

            return new LibraryScanResult(
                items,
                false,
                new[] { errorMessage },
                warnings);
        }
    }
}
