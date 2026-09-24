using CorePlanetMusicPlayer.Core.Artwork;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Mapping
{
    public static class ArtworkAssignmentDataMapper
    {
        public static ArtworkAssignment ToModel(ArtworkAssignmentEntity entity)
        {
            Guard.NotNull(entity, nameof(entity));

            var owner = new ArtworkOwner((ArtworkOwnerKind)entity.OwnerKind, entity.OwnerId);

            var updatedAt = DataValueConverter.FromUnixTimeMilliseconds(entity.UpdatedAtUnixTimeMilliseconds);

            var sourceKind = (ArtworkSourceKind)entity.SourceKind;

            switch (sourceKind)
            {
                case ArtworkSourceKind.ManagedFile:
                    return ArtworkAssignment.CreateManagedFile(owner, entity.ResourceKey,updatedAt);

                case ArtworkSourceKind.RemoteUri:
                    return ArtworkAssignment.CreateRemoteUri(owner, entity.RemoteUrl, updatedAt);

                default:
                    throw new InvalidOperationException("Unsupported stored artwork source kind.");
            }
        }

        public static ArtworkAssignmentEntity ToEntity(ArtworkAssignment assignment)
        {
            Guard.NotNull(assignment, nameof(assignment));

            return new ArtworkAssignmentEntity
            {
                OwnerKind = (int)assignment.Owner.Kind,
                OwnerId = assignment.Owner.Id,
                SourceKind = (int)assignment.SourceKind,
                ResourceKey = assignment.ResourceKey,
                RemoteUrl = assignment.RemoteUrl,
                UpdatedAtUnixTimeMilliseconds = DataValueConverter.ToUnixTimeMilliseconds(assignment.UpdatedAt)
            };
        }
    }
}
