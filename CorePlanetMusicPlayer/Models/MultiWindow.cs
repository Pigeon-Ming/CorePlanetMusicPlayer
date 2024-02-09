using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace CorePlanetMusicPlayer.Models
{
    public class MultiWindow
    {
        public int WindowID {  get; set; }
        public AppWindow window { get; set; }
    }

    public class MultiWindowManager
    {
        public static List<MultiWindow> multiWindows { get; set; }
        public static int CurrentWindowID {  get; set; }

        public static void InitMultiWindowManager()
        {
            multiWindows = new List<MultiWindow>();
            CurrentWindowID = 0;
        }

        public static async Task<int> CreateWindowAsync(String windowTitle,Type pageType)
        {
            MultiWindow multiWindow = new MultiWindow();
            multiWindow.window = await AppWindow.TryCreateAsync();
            Frame appWindowContentFrame = new Frame();
            appWindowContentFrame.Navigate(pageType);
            ElementCompositionPreview.SetAppWindowContent(multiWindow.window, appWindowContentFrame);
            multiWindow.window.Title = windowTitle;
            multiWindow.WindowID = CurrentWindowID++;
            multiWindow.window.TryShowAsync();
            multiWindows.Add(multiWindow);
            return multiWindow.WindowID;
        }
    }
}
