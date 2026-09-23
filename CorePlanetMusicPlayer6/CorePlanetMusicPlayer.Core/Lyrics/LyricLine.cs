using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Lyrics
{
    public class LyricLine
    {
        public int Index { get; set; }

        public TimeSpan Time { get; set; }

        public string Text { get; set; }

        public string TranslationText { get; set; }

        public bool HasText
        {
            get { return !string.IsNullOrWhiteSpace(Text); }
        }

        public bool HasTranslation
        {
            get { return !string.IsNullOrWhiteSpace(TranslationText); }
        }

        public static LyricLine Create(int index, TimeSpan time, string text)
        {
            Guard.NotNegative(index, nameof(index));
            Guard.NotNegative(time, nameof(time));

            return new LyricLine
            {
                Index = index,
                Time = time,
                Text = text
            };
        }

        public static LyricLine Create(int index, TimeSpan time, string text, string translationText)
        {
            Guard.NotNegative(index, nameof(index));
            Guard.NotNegative(time, nameof(time));

            return new LyricLine
            {
                Index = index,
                Time = time,
                Text = text,
                TranslationText = translationText
            };
        }
    }
}
