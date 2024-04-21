using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Models
{
    public class EventList<T>:List<T>
    {
        public event EventHandler<EventListEventArgs<T>> OnChanging = null;

        public new void Add(T item)
        {
            base.Add(item);
            if (OnChanging != null)
            {
                OnChanging.Invoke(this, new EventListEventArgs<T>(item, this.Count));
            }
        }

        public void SetItems(EventList<T>items)
        {
            //items.CopyTo(base.ToArray());
            //base.Clear();
            base.AddRange(items);
            if (OnChanging != null)
            {
                OnChanging.Invoke(this, new EventListEventArgs<T>(items[0], this.Count));
            }
        }

        public static EventList<T> ListToEventList(List<T> list)
        {
            EventList<T> eventList = new EventList<T>();
            for (int i = 0; i < list.Count; i++)
            {
                eventList.Add(list[i]);
            }
            return eventList;
        }

        public static List<T> EventListToList(EventList<T> eventList)
        {
            List<T> list = new List<T>();
            for (int i = 0; i < eventList.Count; i++)
            {
                list.Add(eventList[i]);
            }
            return list;
        }
    }




    public class EventListEventArgs<T> : EventArgs
    {
        public EventListEventArgs(T item, int index)
        {
            Item = item;
            Index = index;
        }
        public T Item { get; set; }
        public int Index { get; set; }

    }
}
