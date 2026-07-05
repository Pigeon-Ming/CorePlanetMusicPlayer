using CorePlanetMusicPlayer.App;
using CorePlanetMusicPlayer.PlayCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UWPTools.Models;
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

namespace CorePlanetMusicPlayer6.Controls
{
    public sealed partial class VolumeControl : UserControl
    {
        IPlayEngine playEngine;

        public VolumeControl()
        {
            this.InitializeComponent();
            playEngine = ProgramData.PlayEngine;
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            playEngine.SetVolume(VolumeSlider.Value / 100);
            SettingsManager.SetSetting("CorePlanetMusicPlayer_Volume", VolumeSlider.Value.ToString());
            updateView();
        }

        private void updateView()
        {
            VolumeLevelTextBlock.Text = ((int)VolumeSlider.Value).ToString();
            UpdateVolumeIcon();
        }

        public void UpdateView()
        {
            VolumeSlider.Value = playEngine.GetVolume() * 100;
            updateView();
        }

        private void UpdateVolumeIcon()
        {
            double volume = VolumeSlider.Value / 100;
            if (volume == 0.0)
                VolumeIcon.Glyph = "\uE992";
            else if (volume <= 0.33)
                VolumeIcon.Glyph = "\uE993";
            else if (volume <= 0.66)
                VolumeIcon.Glyph = "\uE994";
            else
                VolumeIcon.Glyph = "\uE995";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateView();
        }
    }
}
