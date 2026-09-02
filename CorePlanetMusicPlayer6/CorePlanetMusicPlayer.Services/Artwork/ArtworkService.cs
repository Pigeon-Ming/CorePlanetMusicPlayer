using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Artwork
{
    public sealed class ArtworkService : IArtworkService
    {
        private readonly IMusicRepository _musicRepository;

        public ArtworkService(IMusicRepository musicRepository)
        {
            Guard.NotNull(musicRepository, nameof(musicRepository));

            _musicRepository = musicRepository;
        }
        public async Task<ArtworkReference> GetArtworkByMusicIdAsync(MusicId musicId)
        {
            ValidateMusicId(musicId);

            var music = await _musicRepository.GetByIdAsync(musicId);

            if (music == null)
            {
                return ArtworkReference.Default(musicId);
            }

            return await GetArtworkAsync(music);
        }

        public Task<ArtworkReference> GetArtworkAsync(Music music)
        {
            if (music == null || music.Id.IsEmpty)
            {
                return Task.FromResult(ArtworkReference.Default());
            }

            if (CanUseEmbeddedArtwork(music))
            {
                return Task.FromResult(ArtworkReference.CreateAuto(music));
            }

            return Task.FromResult(ArtworkReference.Default(music.Id));
        }

        public Task<ArtworkReference> GetCacheArtworkAsync(MusicId musicId)
        {
            ValidateMusicId(musicId);

            return Task.FromResult(ArtworkReference.Cache(musicId));
        }

        public Task<ArtworkReference> GetEmbeddedArtworkAsync(Music music)
        {
            if (music == null || music.Id.IsEmpty)
            {
                return Task.FromResult(ArtworkReference.Default());
            }

            if (!CanUseEmbeddedArtwork(music))
            {
                return Task.FromResult(ArtworkReference.Default(music.Id));
            }

            return Task.FromResult(ArtworkReference.Embedded(music));
        }

        public Task<ArtworkReference> GetDefaultArtworkAsync()
        {
            return Task.FromResult(ArtworkReference.Default());
        }

        public Task<ArtworkReference> GetDefaultArtworkAsync(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                return Task.FromResult(ArtworkReference.Default());
            }

            return Task.FromResult(ArtworkReference.Default(musicId));
        }

        private void ValidateMusicId(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("Music id cannot be empty.", nameof(musicId));
            }
        }

        private bool CanUseEmbeddedArtwork(Music music)
        {
            if (music == null)
            {
                return false;
            }

            if (music.SourceType != MusicSourceType.Local && music.SourceType != MusicSourceType.Temporary)
            {
                return false;
            }

            if (music.FileInfo == null)
            {
                return false;
            }

            if (music.FileInfo.HasPath)
            {
                return true;
            }

            if (music.FileInfo.HasRelativePath && music.FileInfo.HasLibraryFolder)
            {
                return true;
            }

            return false;
        }
    }
}
