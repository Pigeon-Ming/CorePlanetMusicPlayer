using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetMusicPlayer.Models
{
    public class PlayQueue
    {
        public static EventList<Music> normalList = new EventList<Music>();
        public static EventList<Music> shuffleList = new EventList<Music>();
        public static int currentMusicIndex = 0;
    }

    public class PlayQueueManager
    {
        public void CreateShufflePlayQueue()
        {
            Random random = new Random();
            List<Music> normalList = PlayQueue.normalList;
            int index=0;
            while(normalList.Count>0)
            {
                index = random.Next(normalList.Count-1);
                PlayQueue.shuffleList.Add(normalList[index]);
                normalList.RemoveAt(index);
            }
            
        }
    }
}
