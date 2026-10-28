using CorePlanetMusicPlayer.Core.Lyrics;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;

namespace CorePlanetMusicPlayer6.Models
{
    public sealed class LyricLineViewModel : INotifyPropertyChanged
    {
        private bool _isCurrent;

        public LyricLineViewModel(LyricLine line)
        {
            Line = line ?? throw new ArgumentNullException(nameof(line));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public LyricLine Line { get; }

        public TimeSpan Time => Line.Time;

        public string Text => Line.Text ?? string.Empty;

        public string TranslationText => Line.TranslationText ?? string.Empty;

        public bool HasTranslation => !string.IsNullOrWhiteSpace(TranslationText);

        public Visibility TranslationVisibility =>
            HasTranslation ? Visibility.Visible : Visibility.Collapsed;

        public bool IsCurrent => _isCurrent;

        public double Opacity => _isCurrent ? 1.0 : 0.2;

        public void SetCurrent(bool isCurrent)
        {
            if (_isCurrent == isCurrent)
            {
                return;
            }

            _isCurrent = isCurrent;
            OnPropertyChanged(nameof(IsCurrent));
            OnPropertyChanged(nameof(Opacity));
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
