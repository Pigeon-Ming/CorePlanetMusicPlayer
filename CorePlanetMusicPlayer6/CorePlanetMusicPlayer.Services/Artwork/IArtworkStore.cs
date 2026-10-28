using CorePlanetMusicPlayer.Core.Artwork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Artwork
{
    public interface IArtworkStore
    {
        // 从当前位置读取 source，但不负责释放 source。
        // 成功后返回应用生成的资源键。
        Task<string> ImportAsync(ArtworkOwnerKind ownerKind, Stream source);

        // 只删除应用管理的图片，不操作用户的原始文件。
        Task DeleteAsync(ArtworkOwnerKind ownerKind, string resourceKey);

        // 不存在时返回 null。
        // 返回的流由调用方释放。
        Task<Stream> OpenReadAsync(ArtworkOwnerKind ownerKind, string resourceKey);
    }
}
