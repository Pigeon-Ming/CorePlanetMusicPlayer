using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls.DevControls
{
    public sealed partial class LyricItemControl : UserControl
    {
        Lyric lyric;
        public LyricItemControl(Lyric lyric)
        {
            this.InitializeComponent();
            this.lyric = lyric;
            LyricTextBlock.Text = lyric.Content;
            if (String.IsNullOrEmpty(lyric.Translation))
                TranslationTextBlock.Visibility = Visibility.Visible;
            else
                TranslationTextBlock.Text = lyric.Translation;
        }

        public void LeftAligened()
        {
            this.HorizontalAlignment = HorizontalAlignment.Left;
            LyricTextBlock.TextAlignment = TextAlignment.Left;
            TranslationTextBlock.TextAlignment = TextAlignment.Left;
        }

        public void RightAligened()
        {
            this.HorizontalAlignment = HorizontalAlignment.Right;
            LyricTextBlock.TextAlignment = TextAlignment.Right;
            TranslationTextBlock.TextAlignment = TextAlignment.Right;
        }

        public void SetAsCurrrent()
        {
            LyricTextBlock.Opacity = 1f;
            TranslationTextBlock.Opacity = 1f;
        }

        public void SetAsNotCurrent()
        {
            LyricTextBlock.Opacity = 0.5f;
            TranslationTextBlock.Opacity = 0.5f;
        }
    }
}
