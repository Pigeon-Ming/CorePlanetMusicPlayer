using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.PlayCore
{
    public class PlayMode
    {
        public class RepeatAll : IPlayMode
        {
            public PlayQueue PlayQueue { get; set; }

            public RepeatAll(PlayQueue playQueue)
            {
                PlayQueue = playQueue;
            }


            public int Next(int index)
            {
                if (index < PlayQueue.NormalQueue.Count - 1)
                    index++;
                else
                    index = 0;
                return index;
            }

            public int Previous(int index)
            {
                if (index > 0)
                    index--;
                else
                    index = PlayQueue.NormalQueue.Count - 1;
                return index;
            }
        }

        public class Shuffle : IPlayMode
        {
            public PlayQueue PlayQueue { get; set; }

            public Shuffle(PlayQueue playQueue)
            {
                PlayQueue = playQueue;
            }

            public int Next(int index)
            {
                if (index < PlayQueue.ShuffleQueue.Count - 1)
                    index++;
                else
                    index = 0;
                return index;
            }

            public int Previous(int index)
            {
                if (index > 0)
                    index--;
                else
                    index = PlayQueue.ShuffleQueue.Count - 1;
                return index;
            }
        }

        public class RepeatOne : IPlayMode
        {
            public PlayQueue PlayQueue {get;set;}
            public RepeatOne(PlayQueue playQueue)
            {
                PlayQueue = playQueue;
            }

            public int Next(int index)
            {
                return index;
            }

            public int Previous(int index)
            {
                return index;
            }
        }

        public class Reverse : IPlayMode
        {
            public PlayQueue PlayQueue { get; set; }
            public Reverse(PlayQueue playQueue)
            {
                this.PlayQueue = playQueue;
            }

            public int Next(int index)
            {
                if (index > 0)
                    index--;
                else
                    index = PlayQueue.NormalQueue.Count - 1;
                return index;
            }

            public int Previous(int index)
            {
                if (index < PlayQueue.NormalQueue.Count - 1)
                    index++;
                else
                    index = 0;
                return index;
            }
        }
    }
}
