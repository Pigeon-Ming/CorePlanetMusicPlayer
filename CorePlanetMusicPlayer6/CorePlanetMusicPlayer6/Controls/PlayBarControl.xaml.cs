using CorePlanetMusicPlayer6.Models;
using CorePlanetMusicPlayer6.ViewModels;
using CorePlanetMusicPlayer6.ViewModels.DataModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
    public sealed partial class PlayBarControl : UserControl
    {
        PlayingControlViewModel ViewModel { get; set; } = new PlayingControlViewModel(ProgramData.PlayEngine);
        public PlayBarControl()
        {
            this.InitializeComponent();
            ViewModel.StateChanged += ViewModel_StateChanged;
        }

        private void ViewModel_StateChanged(object sender, EventArgs e)
        {
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ()=>
            {
                Bindings.Update();
            });
        }

        private void PlayModeButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PlayEngine.GetPlayQueue().NextPlayMode();
        }
    }
}
