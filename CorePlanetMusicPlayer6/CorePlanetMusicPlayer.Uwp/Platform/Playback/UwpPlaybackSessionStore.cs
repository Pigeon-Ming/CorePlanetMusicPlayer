using CorePlanetMusicPlayer.Services.Playback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace CorePlanetMusicPlayer.Uwp.Platform.Playback
{
    public sealed class UwpPlaybackSessionStore : IPlaybackSessionStore
    {
        private const string FileName = "playback-session.json";

        public async Task<PlaybackSessionData> LoadAsync()
        {
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName) as StorageFile;

            if (file == null)
            {
                return null;
            }

            using (var stream = await file.OpenStreamForReadAsync())
            {
                var serializer = new DataContractJsonSerializer(typeof(PlaybackSessionData));

                return (PlaybackSessionData)serializer.ReadObject(stream);
            }
        }

        public async Task SaveAsync(PlaybackSessionData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            byte[] bytes;

            // 先完成序列化，避免序列化失败影响原文件。
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(PlaybackSessionData));

                serializer.WriteObject(stream, data);
                bytes = stream.ToArray();
            }

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(FileName, CreationCollisionOption.OpenIfExists);

            using (var transaction = await file.OpenTransactedWriteAsync())
            using (var writer = new DataWriter(transaction.Stream))
            {
                writer.WriteBytes(bytes);

                transaction.Stream.Size = await writer.StoreAsync();

                await transaction.CommitAsync();
            }
        }
    }
}
