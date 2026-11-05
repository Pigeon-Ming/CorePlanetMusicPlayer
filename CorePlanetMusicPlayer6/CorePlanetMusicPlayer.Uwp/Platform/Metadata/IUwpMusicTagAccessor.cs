using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Services.Metadata;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Uwp.Platform.Metadata
{
    public interface IUwpMusicTagAccessor
    {
        Task<MusicTagReadResult> ReadAsync(StorageFile file);

        Task<Result> WriteAsync(
            StorageFile file,
            MusicMetadataUpdateRequest request);
    }
}
