using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Models
{
    public class Artist
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<Music> Music { get; set; } = new List<Music>();
    }

    public class ArtistManager
    {
        public static void AddMusicToArtist(Music music)
        {
            List<string> artists = GetArtistNamesFromArtistString(music.Artist);

            //Debug.WriteLine(music.Title);

            for (int i = 0; i < artists.Count; i++)
            {
                Artist artist = Library.Artists.Find(x => x.Name == artists[i]);
                if (artist != null)
                {
                    artist.Music.Add(music);
                }
                else
                {
                    artist = new Artist();
                    artist.Name = artists[i];
                    artist.Music.Add(music);
                    Library.Artists.Add(artist);
                }

            }

        }

        public static List<string> GetArtistNamesFromArtistString(string ArtistString)
        {
            List<string> artists = new List<string>();
            ArtistString = ArtistString.Replace("; ", ";");
            int semicolonIndex = ArtistString.IndexOf(';');
            if (semicolonIndex == -1)
                artists.Add(ArtistString);
            else
            {
                do
                {
                    artists.Add(ArtistString.Substring(0, semicolonIndex));
                    ArtistString = ArtistString.Substring(semicolonIndex + 1);
                    semicolonIndex = ArtistString.IndexOf(";");
                } while (semicolonIndex != -1);
                artists.Add(ArtistString);
            }
            return artists;
        }


        public static async Task<BitmapImage> GetArtistProfileImageAsync(Artist artist)
        {
            BitmapImage image = null;
            StorageFolder folder = await StorageManager.GetApplicationDataFolder("Images");
            folder = await StorageManager.GetFolder(folder, "Artists");
            if (folder == null) return null;
            IStorageItem item = await folder.TryGetItemAsync(artist.Name + ".jpg");
            if (item == null)
                item = await folder.TryGetItemAsync(artist.Name + ".png") as StorageFile;
            if (item == null)
                return null;

            var stream = await (item as StorageFile).OpenReadAsync();

            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            image = new BitmapImage();
            image.SetSource(stream);
            return image;
        }

        public static async Task SetArtistProfileImageAsync(Artist artist)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                StorageFolder folder = await StorageManager.GetApplicationDataFolder("Images");
                folder = await StorageManager.GetFolder(folder, "Artists");
                if (await StorageManager.IsItemExsitAsync(folder, artist.Name + ".jpg"))
                {
                    await (await folder.GetFileAsync(artist.Name + ".jpg")).DeleteAsync();
                }
                else if (await StorageManager.IsItemExsitAsync(folder, artist.Name + ".png"))
                {
                    await (await folder.GetFileAsync(artist.Name + ".png")).DeleteAsync();
                }
                await file.CopyAsync(folder, artist.Name + file.FileType);
            }

        }
    }
}
