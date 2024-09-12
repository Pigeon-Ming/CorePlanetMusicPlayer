using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Models.TagLibHelper
{
    //Copy from https://github.com/mono/taglib-sharp/issues/177

    public class UwpStorageFileAbstraction : TagLib.File.IFileAbstraction
    {
        private readonly StorageFile file;

        public string Name => file.Name;

        public Stream ReadStream
        {
            get
            {
#pragma warning disable DF0023 // Disposing of this is handled by the TagLibSharp lib by calling the CloseStream method defined here
                return file.OpenStreamForReadAsync().GetAwaiter().GetResult();
#pragma warning restore DF0023 // Disposing of this is handled by the TagLibSharp lib by calling the CloseStream method defined here
            }
        }

        public Stream WriteStream
        {
            get
            {

#pragma warning disable DF0023 // Disposing of this is handled by the TagLibSharp lib by calling the CloseStream method defined here
                return file.OpenStreamForWriteAsync().GetAwaiter().GetResult();
#pragma warning restore DF0023 // Disposing of this is handled by the TagLibSharp lib by calling the CloseStream method defined here
            }
        }


        public UwpStorageFileAbstraction(StorageFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            this.file = file;
        }


        public void CloseStream(Stream stream)
        {
            stream?.Dispose();
        }
    }
}
