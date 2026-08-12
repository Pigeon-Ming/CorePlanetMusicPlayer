using CorePlanetMusicPlayer.Core.History;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Mapping
{
    public static class PlaybackHistoryDataMapper
    {
        public static PlaybackHistoryItem ToModel(PlaybackHistoryEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new PlaybackHistoryItem
            {
                Id = new PlaybackHistoryId(entity.Id),
                MusicId = new MusicId(entity.MusicId),
                PlayedAt = DataValueConverter.FromUnixTimeMilliseconds(entity.PlayedAtUnixTimeMilliseconds),
                MusicDuration = new TimeSpan(entity.MusicDurationTicks),
                PlayedDuration = new TimeSpan(entity.PlayedDurationTicks),
                LastPosition = new TimeSpan(entity.LastPositionTicks),
                IsCompleted = entity.IsCompleted
            };
        }

        public static PlaybackHistoryEntity ToEntity(PlaybackHistoryItem item)
        {
            if (item == null)
            {
                return null;
            }

            return new PlaybackHistoryEntity
            {
                Id = item.Id.ToString(),
                MusicId = item.MusicId.ToString(),
                PlayedAtUnixTimeMilliseconds = item.PlayedAt.ToUnixTimeMilliseconds(),
                MusicDurationTicks = item.MusicDuration.Ticks,
                PlayedDurationTicks = item.PlayedDuration.Ticks,
                LastPositionTicks = item.LastPosition.Ticks,
                IsCompleted = item.IsCompleted
            };
        }
    }
}
