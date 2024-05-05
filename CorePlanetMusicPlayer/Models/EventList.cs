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


        public new void Insert(int index,T item)
        {
            base.Insert(index,item);
            if (OnChanging != null)
            {
                OnChanging.Invoke(this, new EventListEventArgs<T>(item, this.Count));
            }
        }

        public void AddRange(List<T> item)
        {
            base.AddRange(item);
            if (OnChanging != null)
            {
                OnChanging.Invoke(this, new EventListEventArgs<T>());
            }
        }

        public void AddRangeAt(int index,List<T> item)
        {
            for(int i=0;i<item.Count;i++)
                base.Insert(index++,item[i]);
            if (OnChanging != null)
            {
                OnChanging.Invoke(this, new EventListEventArgs<T>());
            }
        }

        public new void Clear()
        {
            base.Clear();
            if (OnChanging != null)
            {
                OnChanging.Invoke(this, new EventListEventArgs<T>());
            }
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            if (OnChanging != null)
            {
                OnChanging.Invoke(this, new EventListEventArgs<T>());
            }
        }

        public void Invoke()
        {
            OnChanging.Invoke(this, new EventListEventArgs<T>());
        }

        public void SetItems(EventList<T>items)
        {
            //items.CopyTo(base.ToArray());
            //base.Clear();
            base.Clear();
            base.AddRange(items.ToList());
            if (OnChanging != null)
            {
                OnChanging.Invoke(this, new EventListEventArgs<T>());
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

        public EventListEventArgs()
        {
            
        }
        public T Item { get; set; }
        public int Index { get; set; }

    }
}
