using CorePlanetMusicPlayer.Models;
using PlanetMusicPlayer.Controls;
using PlanetMusicPlayer.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace PlanetMusicPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static DispatcherTimer timer = new DispatcherTimer();
        public static DispatcherTimer initTimer = new DispatcherTimer();
        public static ContentDialog dialog = new ContentDialog();

        public MainPage()
        {
            this.InitializeComponent();
            LibraryManager.ReloadLibraryAsync();
            MultiWindowManager.InitMultiWindowManager();

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            initTimer.Interval = TimeSpan.FromSeconds(1);
            initTimer.Tick += InitTimer_Tick;
            initTimer.Start();
            TopBar_PostionSlider.AddHandler(UIElement.PointerReleasedEvent /*哪个事件*/, new PointerEventHandler(TopBar_PostionSlider_PointerReleased) /*使用哪个函数处理*/, true /*如果在之前处理，是否还使用函数*/);
            PlayQueue.normalList.OnChanging += NormalList_OnChanging;

        }

        private void InitTimer_Tick(object sender, object e)
        {
            if (PlayCore.MainMediaPlayer.MediaPlayer != null)
            {
                initTimer.Stop();
                timer.Start();
            }
        }

        private void NormalList_OnChanging(object sender, EventListEventArgs<Music> e)
        {
            playQueueListView.ItemsSource = null;
            if (PlayCore.ShufflePlayMode == PlayCore.ShufflePlayModeEnum.None)
                playQueueListView.ItemsSource = PlayQueue.normalList;
            else
                playQueueListView.ItemsSource = PlayQueue.shuffleList;
            playQueueListView.SelectedIndex = PlayQueue.currentMusicIndex;


        }

        private void Timer_Tick(object sender, object e)
        {
            Main_CommandBar_MusicMessage.Text = "正在播放：" + PlayCore.CurrentMusic.Title + " - " + PlayCore.CurrentMusic.Artist;
            TopBar_PostionSlider.Maximum = PlayCore.MainMediaPlayer.MediaPlayer.NaturalDuration.TotalSeconds;
            TopBar_PostionSlider.Value = PlayCore.MainMediaPlayer.MediaPlayer.Position.TotalSeconds;
            TopBar_CurrentPosition.Text = PlayCore.MainMediaPlayer.MediaPlayer.Position.ToString().Substring(3, 5);
            TopBar_TotalPosition.Text = PlayCore.MainMediaPlayer.MediaPlayer.NaturalDuration.ToString().Substring(3, 5);
        }

        private void MenuBar_File_RefreshLibrary(object sender, RoutedEventArgs e)
        {

            libraryListView.ItemsSource = null;
            libraryListView.ItemsSource = Library.LocalLibraryMusic;
            albumsListView.ItemsSource = null;
            albumsListView.ItemsSource = AlbumManager.Albums;
            artistsListView.ItemsSource = null;
            artistsListView.ItemsSource = ArtistManager.Artists;
            playQueueListView.ItemsSource = null;
            if (PlayCore.ShufflePlayMode == PlayCore.ShufflePlayModeEnum.None)
                playQueueListView.ItemsSource = PlayQueue.normalList;
            else
                playQueueListView.ItemsSource = PlayQueue.shuffleList;
            playQueueListView.SelectedIndex = PlayQueue.currentMusicIndex;
            //Debug.WriteLine(Library.LocalLibraryMusic.Count);
        }

        private void Library_CommandBar_Play(object sender, RoutedEventArgs e)
        {
            PlayCore.PlayMusic((Music)libraryListView.SelectedItem, Library.LocalLibraryMusic, libraryListView.SelectedIndex);
            timer.Start();

        }


        private void Main_CommandBar_Pause(object sender, RoutedEventArgs e)
        {
            PlayCore.PauseMusic();
        }

        private void Main_CommandBar_Play(object sender, RoutedEventArgs e)
        {
            PlayCore.PlayMusic();
        }

        private void Main_CommandBar_Previous(object sender, RoutedEventArgs e)
        {
            PlayCore.PreviousMusic();
        }

        private void Main_CommandBar_Next(object sender, RoutedEventArgs e)
        {
            PlayCore.NextMusic();
        }



        private void TopBar_PostionSlider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (PlayCore.MainMediaPlayer.MediaPlayer != null)
                PlayCore.MainMediaPlayer.MediaPlayer.Position = TimeSpan.FromSeconds(TopBar_PostionSlider.Value);
        }

        private void TopBar_PostionSlider_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (PlayCore.MainMediaPlayer.MediaPlayer != null)
                PlayCore.MainMediaPlayer.MediaPlayer.Position = TimeSpan.FromSeconds(TopBar_PostionSlider.Value);
        }

        private void MenuBar_DevTool_Lyric_Click(object sender, RoutedEventArgs e)
        {
            _ = MultiWindowManager.CreateWindowAsync("歌词工具", typeof(DevLyricPage));
        }

        private void MenuBar_DevTool_LocalFolder_Click(object sender, RoutedEventArgs e)
        {
            _ = MultiWindowManager.CreateWindowAsync("内部文件夹工具", typeof(LocalFolderPage));
        }

        private void PlayQueue_Refresh_Click(object sender, RoutedEventArgs e)
        {
            playQueueListView.ItemsSource = null;
            Debug.WriteLine("PlayQueueCount" + PlayQueue.normalList.Count);
            if (PlayCore.ShufflePlayMode == PlayCore.ShufflePlayModeEnum.None)
                playQueueListView.ItemsSource = PlayQueue.normalList;
            else
                playQueueListView.ItemsSource = PlayQueue.shuffleList;
            playQueueListView.SelectedIndex = PlayQueue.currentMusicIndex;
        }

        private void albumsListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _ = MultiWindowManager.CreateWindowAsync("专辑-" + ((Album)albumsListView.SelectedItem).Name, new AlbumViewPage((Album)albumsListView.SelectedItem));
            //Debug.WriteLine(((Album)albumsListView.SelectedItem).Name);
            //Frame.Navigate(typeof(AlbumViewPage), ((Album)albumsListView.SelectedItem));
        }

        private void playlist_CommandBar_Create(object sender, RoutedEventArgs e)
        {
            PlaylistManager.CreatePlaylistAsync(playlist_CreateName.Text);
        }

        private async void playlist_CommandBar_Refresh(object sender, RoutedEventArgs e)
        {
            List<Playlist> lists = await PlaylistManager.GetPlaylistsListAsync();
            PlaylistListView.ItemsSource = lists;
        }

        private void Main_CommandBar_SaveToPlaylist(object sender, RoutedEventArgs e)
        {
            dialog = new ContentDialog();
            dialog.Content = new SaveToPlaylistControl(PlayCore.CurrentMusic);
            dialog.ShowAsync();
        }

        private void PlaylistListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            
            MultiWindowManager.CreateWindowAsync("播放列表-"+ ((Playlist)e.ClickedItem).Name,new PlaylistPage(((Playlist)e.ClickedItem).Name));
        }
    }
}
