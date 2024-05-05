using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Models
{
    public class PlayQueue
    {
        public static EventList<Music> normalList = new EventList<Music>();
        public static EventList<Music> shuffleList = new EventList<Music>();
        public static int currentMusicIndex = 0;
    }

    public class PlayQueueManager
    {
        public static void AddMusicPlayNext(Music music)
        {
            if(PlayCore.ShufflePlayMode == PlayCore.ShufflePlayModeEnum.All|| PlayCore.ShufflePlayMode == PlayCore.ShufflePlayModeEnum.NoRepeat)
            {
                PlayQueue.shuffleList.Insert(PlayQueue.currentMusicIndex+1,music);
            }
            else
            {
                PlayQueue.normalList.Insert(PlayQueue.currentMusicIndex + 1, music);
            }
        }

        public static void AddMusicToPlayQueue(Music music)
        {
            if (PlayCore.ShufflePlayMode == PlayCore.ShufflePlayModeEnum.All || PlayCore.ShufflePlayMode == PlayCore.ShufflePlayModeEnum.NoRepeat)
            {
                PlayQueue.shuffleList.Add(music);
            }
            else
            {
                PlayQueue.normalList.Add(music);
            }
        }

        public static void AddMusicListToPlayQueue(List<Music> music)
        {
            if (PlayCore.ShufflePlayMode == PlayCore.ShufflePlayModeEnum.All || PlayCore.ShufflePlayMode == PlayCore.ShufflePlayModeEnum.NoRepeat)
            {
                PlayQueue.shuffleList.AddRange(music);
                
            }
            else
            {
                PlayQueue.normalList.AddRange(music);
                
            }
        }

        public static void CreateShufflePlayQueue()
        {
            Random random = new Random();
            PlayQueue.shuffleList.Clear();
            List<Music> normalList = PlayQueue.normalList.ToList();
            List<Music> newList = new List<Music>();
            int index=0;
            while(normalList.Count>0)
            {
                index = random.Next(normalList.Count-1);
                newList.Add(normalList[index]);
                normalList.RemoveAt(index);
            }

            PlayQueue.shuffleList.SetItems(EventList<Music>.ListToEventList(newList));


        }
    }
}
