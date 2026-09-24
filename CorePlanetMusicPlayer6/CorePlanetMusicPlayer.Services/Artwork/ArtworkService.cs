using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Artwork;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Core.Playlists;
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
        private const string DefaultMusicResourceName = "DefaultAlbumArtwork";

        private const string DefaultArtistResourceName = "DefaultArtistArtwork";

        private const string DefaultPlaylistResourceName = "DefaultPlaylistArtwork";

        private readonly IMusicRepository _musicRepository;
        private readonly IArtistRepository _artistRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IArtworkRepository _artworkRepository;

        public ArtworkService(
            IMusicRepository musicRepository,
            IArtistRepository artistRepository,
            IPlaylistRepository playlistRepository,
            IArtworkRepository artworkRepository)
        {
            Guard.NotNull(musicRepository, nameof(musicRepository));
            Guard.NotNull(artistRepository, nameof(artistRepository));
            Guard.NotNull(playlistRepository, nameof(playlistRepository));
            Guard.NotNull(artworkRepository, nameof(artworkRepository));

            _musicRepository = musicRepository;
            _artistRepository = artistRepository;
            _playlistRepository = playlistRepository;
            _artworkRepository = artworkRepository;
        }

        public async Task<ArtworkReference> GetArtworkByMusicIdAsync(MusicId musicId)
        {
            ValidateMusicId(musicId);

            var music = await _musicRepository.GetByIdAsync(musicId);

            if (music == null)
            {
                return await GetDefaultArtworkAsync(musicId);
            }

            return await GetArtworkAsync(music);
        }

        public async Task<ArtworkReference> GetArtworkAsync(Music music)
        {
            if (music == null || music.Id.IsEmpty)
            {
                return await GetDefaultArtworkAsync();
            }

            if (music.SourceType == MusicSourceType.Local ||
                music.SourceType == MusicSourceType.Temporary)
            {
                return ArtworkReference.FromMusicFile(
                    music,
                    DefaultMusicResourceName);
            }

            if (music.SourceType == MusicSourceType.Stream)
            {
                var owner = new ArtworkOwner(ArtworkOwnerKind.Music, music.Id.ToString());

                return await GetAssignedOrDefaultAsync(owner, DefaultMusicResourceName);
            }

            return await GetDefaultArtworkAsync(music.Id);
        }

        public async Task<ArtworkReference> GetArtworkByArtistIdAsync(ArtistId artistId)
        {
            if (artistId.IsEmpty)
            {
                throw new ArgumentException("艺术家 ID 不能为空。", nameof(artistId));
            }

            var owner = new ArtworkOwner(ArtworkOwnerKind.Artist, artistId.ToString());

            var artist = await _artistRepository.GetByIdAsync(artistId);

            if (artist == null)
            {
                return ArtworkReference.Default(DefaultArtistResourceName, owner);
            }

            return await GetAssignedOrDefaultAsync(owner, DefaultArtistResourceName);
        }

        public async Task<ArtworkReference> GetArtworkByPlaylistIdAsync(PlaylistId playlistId)
        {
            if (playlistId.IsEmpty)
            {
                throw new ArgumentException("播放列表 ID 不能为空。", nameof(playlistId));
            }

            var owner = new ArtworkOwner(ArtworkOwnerKind.Playlist, playlistId.ToString());

            var playlist = await _playlistRepository.GetByIdAsync(playlistId);

            if (playlist == null)
            {
                return ArtworkReference.Default(DefaultPlaylistResourceName, owner);
            }

            return await GetAssignedOrDefaultAsync(owner, DefaultPlaylistResourceName);
        }

        public Task<ArtworkReference> GetDefaultArtworkAsync()
        {
            return Task.FromResult(ArtworkReference.Default(DefaultMusicResourceName));
        }

        public Task<ArtworkReference> GetDefaultArtworkAsync(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                return GetDefaultArtworkAsync();
            }

            var owner = new ArtworkOwner(ArtworkOwnerKind.Music, musicId.ToString());

            return Task.FromResult(ArtworkReference.Default(DefaultMusicResourceName, owner));
        }

        private async Task<ArtworkReference> GetAssignedOrDefaultAsync(ArtworkOwner owner, string defaultResourceName)
        {
            var assignment = await _artworkRepository.GetByOwnerAsync(owner);

            if (assignment == null)
            {
                return ArtworkReference.Default(defaultResourceName, owner);
            }

            return ArtworkReference.FromAssignment(assignment, defaultResourceName);
        }

        private static void ValidateMusicId(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("音乐 ID 不能为空。", nameof(musicId));
            }
        }
    }
}
