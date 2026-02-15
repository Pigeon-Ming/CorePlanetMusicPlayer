using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace CorePlanetMusicPlayer.App
{
    public static class NavigateManager
    {

        public static Frame ContentFrame { get; set; }

        public static Type AlbumPage { get; set; }

        public static Type ArtistPage { get; set; }

        public static Type YearPage { get; set; }

        public static Type PlaylistPage { get; set; }

        public static Type SearchPage { get; set; }

        public static Type ArtistsSelectControl { get; set; }

        public static Type SaveToPlaylistControl { get; set; }

        public static Type PlaylistEditControl { get; set; }

        public static Type MusicInfoControl { get; set; }

        

        public static void NavigateToAlbumPage(Album album)
        {
            ContentFrame.Navigate(AlbumPage, album);
        }

        public static void NavigateToArtistPage(Artist artist)
        {
            ContentFrame.Navigate(ArtistPage, artist);
        }

        public static void NavigateToYearPage(Year year)
        {
            ContentFrame.Navigate(YearPage, year);
        }

        public static void NavigateToPlaylistPage(Playlist playlist)
        {
            ContentFrame.Navigate(PlaylistPage, playlist);
        }

        public static async void NavigateToArtistPage(List<Artist> artists)
        {
            if (artists == null || artists.Count <= 0)
                return;
            if (artists.Count > 1)
            {

                Type[] parameterTypes = new Type[] { typeof(List<Artist>) };
                ConstructorInfo parameterizedConstructor = ArtistsSelectControl.GetConstructor(parameterTypes);
                if (parameterizedConstructor != null)
                {
                    object[] parameters = new object[] { artists };
                    object instance = parameterizedConstructor.Invoke(parameters);
                    ((IArtistsSelectControl)instance).ArtistSelected += NavigateManager_ArtistSelected;
                    await ProgramData.ContentDialogManager.ShowContentDialogAsync(instance);
                }
            }
            else
            {
                NavigateToArtistPage(artists.First());
            }
        }

        public static void NavigateToSearchPage(string searchingString)
        {
            if (String.IsNullOrEmpty(searchingString))
                return;
            ContentFrame.Navigate(SearchPage, searchingString);
        }

        public static async void SaveToPlaylist(IMusic music)
        {
            Type[] parameterTypes = new Type[] { typeof(IMusic) };
            ConstructorInfo parameterizedConstructor = SaveToPlaylistControl.GetConstructor(parameterTypes);
            if (parameterizedConstructor != null)
            {
                object[] parameters = new object[] { music };
                object instance = parameterizedConstructor.Invoke(parameters);
                await ProgramData.ContentDialogManager.ShowContentDialogAsync(instance);
            }
        }

        public static async void SaveToPlaylist(List<IMusic> musicList)
        {
            Type[] parameterTypes = new Type[] { typeof(List<IMusic>) };
            ConstructorInfo parameterizedConstructor = SaveToPlaylistControl.GetConstructor(parameterTypes);
            if (parameterizedConstructor != null)
            {
                object[] parameters = new object[] { musicList };
                object instance = parameterizedConstructor.Invoke(parameters);
                await ProgramData.ContentDialogManager.ShowContentDialogAsync(instance);
            }
        }

        public static async void ShowMusicInfo(IMusic music)
        {
            Type[] parameterTypes = new Type[] { typeof(IMusic) };
            ConstructorInfo parameterizedConstructor = MusicInfoControl.GetConstructor(parameterTypes);
            if (parameterizedConstructor != null)
            {
                object[] parameters = new object[] { music };
                object instance = parameterizedConstructor.Invoke(parameters);
                await ProgramData.ContentDialogManager.ShowContentDialogAsync(instance);
            }
        }

        public static async void EditPlaylist(Playlist playlist)
        {
            Type[] parameterTypes = new Type[] { typeof(Playlist) };
            ConstructorInfo parameterizedConstructor = PlaylistEditControl.GetConstructor(parameterTypes);
            if (parameterizedConstructor != null)
            {
                object[] parameters = new object[] { playlist };
                object instance = parameterizedConstructor.Invoke(parameters);
                await ProgramData.ContentDialogManager.ShowContentDialogAsync(instance);
            }
        }

        private static void NavigateManager_ArtistSelected(object sender, Artist e)
        {
            ((IArtistsSelectControl)sender).ArtistSelected -= NavigateManager_ArtistSelected;
            NavigateToArtistPage(e);
        }
    }
}
