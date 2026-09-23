using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace CorePlanetMusicPlayer.Uwp.Platform.Storage
{
    public sealed class UwpFolderPickerService
    {
        public async Task<StorageFolder> PickFolderAsync()
        {
            var picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            picker.FileTypeFilter.Add("*");

            return await picker.PickSingleFolderAsync();
        }
    }
}
