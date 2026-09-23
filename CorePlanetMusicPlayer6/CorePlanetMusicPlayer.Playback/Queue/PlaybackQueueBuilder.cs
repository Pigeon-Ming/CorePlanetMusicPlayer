using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Queue
{
    public sealed class PlaybackQueueBuilder
    {
        public PlaybackQueue Build(IEnumerable<MusicId> musicIds)
        {
            var queue = new PlaybackQueue();

            queue.SetItems(musicIds);

            return queue;
        }

        public PlaybackQueue Build(IEnumerable<MusicId> musicIds, MusicId startMusicId)
        {
            var queue = new PlaybackQueue();

            queue.SetItems(musicIds);

            if (!startMusicId.IsEmpty)
            {
                queue.SetCurrent(startMusicId);
            }

            return queue;
        }

        public PlaybackQueue BuildSingle(MusicId musicId)
        {
            var musicIds = new List<MusicId>();

            if(!musicId.IsEmpty)
            {
                musicIds.Add(musicId);
            }

            return Build(musicIds, musicId);
        }

        public PlaybackQueue Restore(PlaybackQueueSnapshot snapshot)
        {
            var queue = new PlaybackQueue();

            queue.Restore(snapshot);

            return queue;
        }
    }
}
