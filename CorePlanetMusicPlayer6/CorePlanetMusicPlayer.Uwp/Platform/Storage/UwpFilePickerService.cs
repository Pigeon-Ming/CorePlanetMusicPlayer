using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace CorePlanetMusicPlayer.Uwp.Platform.Storage
{
    public sealed class UwpFilePickerService
    {
        public async Task<StorageFile> PickMusicFileAsync()
        {
            var picker = CreateMusicFilePicker();


        }

        private static FileOpenPicker CreateMusicFilePicker()
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".flac");
            picker.FileTypeFilter.Add(".wav");
            picker.FileTypeFilter.Add(".m4a");
            picker.FileTypeFilter.Add(".aac");
            picker.FileTypeFilter.Add(".wma");
            picker.FileTypeFilter.Add(".ogg");

            return picker;
        }
    }
}
