using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Models
{
    public class Artist
    {
        public String Name { get; set; }
        public List<Music> IncludeMusic { get; set; }
        public String Profile { get; set; }
        public BitmapImage ProfileImage { get; set; }
    }

    public class ArtistManager
    {
        public static List<Artist> Artists = new List<Artist>();
        public static List<Artist> ArtistSuggestions = new List<Artist>();

        

        public static async Task ClassifyArtist()//待优化
        {
            List<string> Name = new List<string>();
            for (int i = 0; i < Library.LocalLibraryMusic.Count; i++)
            {
                Debug.WriteLine("i="+i);
                Debug.WriteLine("artist="+ Library.LocalLibraryMusic[i].Artist);
                List<string> currentArtistName = new List<string>();
                //int currentArtistName_stringindex = 0;
                string fullArtistName = Library.LocalLibraryMusic[i].Artist;
                fullArtistName = fullArtistName.Replace("; ",";");
                while (true)
                {
                    if (fullArtistName.IndexOf(";") != -1)
                    {
                        currentArtistName.Add(fullArtistName.Substring(0, fullArtistName.IndexOf(";")));
                        fullArtistName = fullArtistName.Substring(fullArtistName.IndexOf(";") + 1, fullArtistName.Length - fullArtistName.IndexOf(";") - 1);
                    }
                    else
                    {
                        currentArtistName.Add(fullArtistName);
                        break;
                    }
                }

                for (int j = 0; j < currentArtistName.Count; j++)
                {

                    if (Name.IndexOf(currentArtistName[j]) == -1)//如果艺术家库里没有这个名字
                    {
                        //Debug.WriteLine("1   "+AllData.SystemLibraryMusic[i].Name);
                        Name.Add(currentArtistName[j]);
                        Artist Artist = new Artist { Name = currentArtistName[j] };
                        Artist.IncludeMusic = new List<Music>();
                        Artist.IncludeMusic.Add(Library.LocalLibraryMusic[i]);
                        Artists.Add(Artist);

                    }
                    else
                    {
                        Artists[Name.IndexOf(currentArtistName[j])].IncludeMusic.Add(Library.LocalLibraryMusic[i]);
                    }
                }
            }
            Artists = Artists.OrderBy(x => x.Name).ToList();
            GetArtistSuggestions();
            if (await Windows.Storage.ApplicationData.Current.LocalFolder.TryGetItemAsync("artists") == null)
            {
                await GetArtistFolderAsync();
                return;
            }

            for (int i = 0; i < Artists.Count; i++)
            {
                Artists[i].ProfileImage = await GetArtistImageAsync(Artists[i].Name);
            }
        }//对艺术家进行分类

        public static Artist FindArtistByName(String Name)
        {
            if(!String.IsNullOrEmpty(Name))
                return Artists.Find(x => x.Name == Name);
            else
                return new Artist { Name="未知艺术家"};
        }

        public static List<Album> GetArtistAlbums(Artist artist)
        {
            List<String>albumNames = new List<String>();
            List<Album> albums = new List<Album>();
            for (int i=0;i<artist.IncludeMusic.Count;i++)
            {
                if (albumNames.IndexOf(artist.IncludeMusic[i].Album) == -1)
                {
                    albumNames.Add(artist.IncludeMusic[i].Album);
                    albums.Add(AlbumManager.FindAlbumByName(artist.IncludeMusic[i].Album));
                }
            }
            return albums;
        }

        public static List<Artist> DivideArtist(String fullArtistName)
        {
            List<Artist> artists = new List<Artist>();
            List<String> ArtistNames = new List<string>();
            while (true)
            {
                if (fullArtistName.IndexOf(";") != -1)
                {
                    ArtistNames.Add(fullArtistName.Substring(0, fullArtistName.IndexOf(";")));
                    if (fullArtistName.IndexOf("; ") == -1)
                    {
                        fullArtistName = fullArtistName.Substring(fullArtistName.IndexOf(";") + 1, fullArtistName.Length - fullArtistName.IndexOf(";") - 1);
                    }
                    else
                    {
                        fullArtistName = fullArtistName.Substring(fullArtistName.IndexOf(";") + 2, fullArtistName.Length - fullArtistName.IndexOf(";") - 2);
                    }
                }
                else
                {
                    ArtistNames.Add(fullArtistName);
                    break;
                }
            }
            for (int i = 0; i < ArtistNames.Count; i++)
                artists.Add(FindArtistByName(ArtistNames[i]));
            return artists;
        }

        public static async Task<StorageFolder> GetArtistFolderAsync()
        {
            StorageFolder folder = (StorageFolder)await Windows.Storage.ApplicationData.Current.LocalFolder.TryGetItemAsync("artists");
            if (folder == null)
            {
                folder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("artists");
            }
            return folder;
        }

        public static async Task<BitmapImage> GetArtistImageAsync(String ArtistName)
        {
            ArtistName = ArtistName.Replace("/", "").Replace("\\", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace("<","").Replace(">","").Replace("|","");
            StorageFolder storageFolder = await GetArtistFolderAsync();
            if (storageFolder == null) return null;
            IStorageItem item = await storageFolder.TryGetItemAsync(ArtistName+".jpg");
            if(item==null)
                item = await storageFolder.TryGetItemAsync(ArtistName + ".png") as StorageFile;
            if(item ==null)
                return null;

            var stream = await (item as StorageFile).OpenReadAsync();

            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            BitmapImage image = new BitmapImage();
            image.SetSource(stream);

            
            return image;
        }

        public static async void GetArtistSuggestions()
        {
            Random random = new Random();
            List<int> indexes = new List<int>();
            if (Artists.Count < 10)
            {
                for (int i = 0; i < Artists.Count; i++)
                    ArtistSuggestions.Add(Artists[i]);
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    int index;
                    while (indexes.Contains(index = random.Next(Artists.Count - 1)) == true) { }
                    indexes.Add(index);
                    
                    ArtistSuggestions.Add(Artists[index]);
                }
            }
            GetSuggestionsArtistImageAsync();
        }

        public static async Task GetSuggestionsArtistImageAsync()
        {
            for(int i = 0; i < ArtistSuggestions.Count; i++)
            {
                if (ArtistSuggestions[i].ProfileImage==null)
                    ArtistSuggestions[i].ProfileImage = await GetArtistImageAsync(ArtistSuggestions[i].Name);
            }
                
        }

        public static async Task SetArtistImageAsync(Artist artist)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

            if (file == null) return ;
            if (string.IsNullOrEmpty(file.Path))
            {
                return ;
            }

            StorageFile file1 = await file.CopyAsync(await GetArtistFolderAsync());
            await file1.RenameAsync(artist.Name.Replace("/", "").Replace("\\", "").Replace(":", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "") +file.FileType);
        }
    }
}
