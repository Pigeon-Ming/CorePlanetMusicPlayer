using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Lyrics
{
    public class LyricDocument
    {
        public string Id { get; set; } = string.Empty;

        public MusicId MusicId { get; set; } 

        public LyricSourceType SourceType { get; set; }

        public string SourcePath { get; set; } = string.Empty;

        public string RawText { get; set; } = string.Empty;

        public List<LyricLine> Lines { get; set; } = new List<LyricLine>();

        public DateTimeOffset CreatedAt { get;set; } = DateTimeOffset.Now;

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

        public bool HasRawText
        {
            get { return !string.IsNullOrWhiteSpace(RawText); }
        }

        public bool HasLines
        {
            get { return Lines != null && Lines.Count > 0; }
        }

        public int LineCount
        {
            get { return Lines == null ? 0 : Lines.Count; }
        }

        public static LyricDocument Create(MusicId musicId, LyricSourceType sourceType, string sourcePath, string rawText)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("MusicId cannot be empty.", nameof(musicId));
            }

            return new LyricDocument
            {
                Id = EntityId.New(),
                MusicId = musicId,
                SourceType = sourceType,
                SourcePath = sourcePath,
                RawText = rawText,
                Lines = new List<LyricLine>(),
                CreatedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now
            };
        }

        public static LyricDocument CreateEmpty(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("MusicId cannot be empty.", nameof(musicId));
            }

            return new LyricDocument
            {
                Id = EntityId.New(),
                MusicId = musicId,
                SourceType = LyricSourceType.Unknown,
                SourcePath = string.Empty,
                RawText = string.Empty,
                Lines = new List<LyricLine>(),
                CreatedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now
            };
        }

        public void SetRawText(string rawText)
        {
            RawText = rawText ?? string.Empty;
            UpdatedAt = DateTimeOffset.Now;
        }

        public void AddLine(LyricLine line)
        {
            Guard.NotNull(line, nameof(line));

            if(Lines == null)
            {
                Lines = new List<LyricLine>();
            }

            Lines.Add(line);
            SortLines();
            UpdatedAt = DateTimeOffset.Now;
        }

        public void SetLines(IEnumerable<LyricLine> lines)
        {
            var sourceLines = lines ?? Enumerable.Empty<LyricLine>();

            Lines = new List<LyricLine>(sourceLines);

            SortLines();
            UpdatedAt = DateTimeOffset.Now;
        }

        public void ClearLines()
        {
            if(Lines == null)
            {
                Lines = new List<LyricLine>();
            }
            else
            {
                Lines.Clear();
            }

            UpdatedAt = DateTimeOffset.Now;
        }

        public LyricLine GetLineAt(TimeSpan position)
        {
            if (Lines == null || Lines.Count == 0)
            {
                return null;
            }

            if(position < TimeSpan.Zero)
            {
                return null;
            }

            LyricLine currentLine = null;

            for (int i = 0; i < Lines.Count; i++)
            {
                if (Lines[i].Time <= position)
                {
                    currentLine = Lines[i];
                }
                else
                {
                    break;
                }
            }

            return currentLine;
        }

        public LyricLine GetNextLine(TimeSpan position)
        {
            if (Lines == null || Lines.Count == 0)
            {
                return null;
            }

            if (position < TimeSpan.Zero)
            {
                return Lines[0];
            }

            for (int i = 0; i < Lines.Count; i++)
            {
                if (Lines[i].Time > position)
                {
                    return Lines[i];
                }
            }

            return null;
        }

        public void SortLines()
        {
            if (Lines == null)
            {
                return;
            }

            Lines.Sort(CompareLyricLines);
        }

        private static int CompareLyricLines(LyricLine left, LyricLine right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Time.CompareTo(right.Time);
        }
    }
}
