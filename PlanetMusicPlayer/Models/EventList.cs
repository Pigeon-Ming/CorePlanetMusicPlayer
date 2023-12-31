﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetMusicPlayer.Models
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
            base.Clear();
            base.AddRange(items);
            if (OnChanging != null && items.Count!=0)
            {
                OnChanging.Invoke(this, new EventListEventArgs<T>(items[0], this.Count));
            }
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
