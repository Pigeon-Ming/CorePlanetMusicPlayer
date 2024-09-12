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

namespace PlanetMusicPlayer.Controls.DevPage
{
    public sealed partial class DevPageControlsShell : UserControl
    {
        FrameworkElement frameworkElement { get; set; }
        public DevPageControlsShell(string headerText,FrameworkElement frameworkElement)
        {
            this.InitializeComponent();
            this.frameworkElement = frameworkElement;
            ControlHeader.Text = headerText;
            Loaded += DevPageControlsShell_Loaded;
        }

        private void DevPageControlsShell_Loaded(object sender, RoutedEventArgs e)
        {
            ContentGrid.Children.Add(frameworkElement);
        }
    }
}
