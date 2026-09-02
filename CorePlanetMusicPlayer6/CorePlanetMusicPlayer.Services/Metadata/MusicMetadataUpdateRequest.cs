using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Metadata
{
    public sealed class MusicMetadataUpdateRequest
    {
        public MusicId MusicId { get; private set; }

        public bool HasTitle { get; private set; }
        public string Title { get; private set; }

        public bool HasArtistName { get; private set; }
        public string ArtistName { get; private set; }

        public bool HasAlbumTitle { get; private set; }
        public string AlbumTitle { get; private set; }

        public bool HasAlbumArtistName { get; private set; }
        public string AlbumArtistName { get; private set; }

        public bool HasGenre { get; private set; }
        public string Genre { get; private set; }

        public bool HasYear { get; private set; }
        public int? Year { get; private set; }

        public bool HasTrackNumber { get; private set; }
        public int? TrackNumber { get; private set; }

        public bool HasDiscNumber { get; private set; }
        public int? DiscNumber { get; private set; }

        public bool HasComposer { get; private set; }
        public string Composer { get; private set; }

        public bool HasComment { get; private set; }
        public string Comment { get; private set; }

        private MusicMetadataUpdateRequest(MusicId musicId)
        {
            MusicId = musicId;
            Title = string.Empty;
            ArtistName = string.Empty;
            AlbumTitle = string.Empty;
            AlbumArtistName = string.Empty;
            Genre = string.Empty;
            Composer = string.Empty;
            Comment = string.Empty;
        }

        public static MusicMetadataUpdateRequest ForMusic(MusicId musicId)
        {
            return new MusicMetadataUpdateRequest(musicId);
        }

        public MusicMetadataUpdateRequest WithTitle(string title)
        {
            HasTitle = true;
            Title = title ?? string.Empty;
            return this;
        }

        public MusicMetadataUpdateRequest WithArtistName(string artistName)
        {
            HasArtistName = true;
            ArtistName = artistName ?? string.Empty;
            return this;
        }

        public MusicMetadataUpdateRequest WithAlbumTitle(string albumTitle)
        {
            HasAlbumTitle = true;
            AlbumTitle = albumTitle ?? string.Empty;
            return this;
        }

        public MusicMetadataUpdateRequest WithAlbumArtistName(string albumArtistName)
        {
            HasAlbumArtistName = true;
            AlbumArtistName = albumArtistName ?? string.Empty;
            return this;
        }

        public MusicMetadataUpdateRequest WithGenre(string genre)
        {
            HasGenre = true;
            Genre = genre ?? string.Empty;
            return this;
        }

        public MusicMetadataUpdateRequest WithYear(int? year)
        {
            HasYear = true;
            Year = year;
            return this;
        }

        public MusicMetadataUpdateRequest WithTrackNumber(int? trackNumber)
        {
            HasTrackNumber = true;
            TrackNumber = trackNumber;
            return this;
        }

        public MusicMetadataUpdateRequest WithDiscNumber(int? discNumber)
        {
            HasDiscNumber = true;
            DiscNumber = discNumber;
            return this;
        }

        public MusicMetadataUpdateRequest WithComposer(string composer)
        {
            HasComposer = true;
            Composer = composer ?? string.Empty;
            return this;
        }

        public MusicMetadataUpdateRequest WithComment(string comment)
        {
            HasComment = true;
            Comment = comment ?? string.Empty;
            return this;
        }

        public bool HasAnyChange
        {
            get
            {
                return HasTitle || HasArtistName || HasAlbumTitle || HasAlbumArtistName || HasGenre || HasYear || HasTrackNumber || HasDiscNumber || HasComposer || HasComment;
            }
        }
    }
}
