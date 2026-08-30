using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Queue
{
    public sealed class PlaybackQueue
    {
        private readonly List<PlaybackQueueItem> _items = new List<PlaybackQueueItem>();

        private int _currentIndex = -1;

        public IReadOnlyList<PlaybackQueueItem> Items
        {
            get { return _items.AsReadOnly(); }
        }

        public int CurrentIndex
        {
            get { return _currentIndex; }
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public bool HasItems
        {
            get { return _items.Count > 0; }
        }

        public bool HasCurrent
        {
            get { return _currentIndex >= 0 && _currentIndex < _items.Count; }
        }

        public void SetItems(IEnumerable<MusicId> musicIds)
        {
            _items.Clear();
            _currentIndex = -1;

            if (musicIds == null)
            {
                return;
            }

            var order = 0;

            foreach (var musicId in musicIds)
            {
                if (musicId.IsEmpty)
                {
                    continue;
                }

                _items.Add(PlaybackQueueItem.Create(musicId, order));
                order++;
            }

            if (_items.Count > 0)
            {
                _currentIndex = 0;
            }
        }

        public void SetItems(IEnumerable<PlaybackQueueItem> items)
        {
            _items.Clear();
            _currentIndex = -1;

            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                if (item == null || item.MusicId.IsEmpty)
                {
                    continue;
                }

                _items.Add(new PlaybackQueueItem
                {
                    Id = item.Id ?? string.Empty,
                    MusicId = item.MusicId,
                    Order = item.Order,
                });
            }

            SortAndReorder();

            if (_items.Count > 0)
            {
                _currentIndex = 0;
            }
        }

        public bool SetCurrent(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                return false;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].MusicId == musicId)
                {
                    _currentIndex = i;
                    return true;
                }
            }

            return false;
        }

        public bool SetCurrentIndex(int index)
        {
            if (index < 0 || index >= _items.Count)
            {
                return false;
            }

            _currentIndex = index;
            return true;
        }

        public PlaybackQueueItem GetCurrentItem()
        {
            if (!HasCurrent)
            {
                return null;
            }

            return _items[_currentIndex];
        }

        public MusicId? GetCurrent()
        {
            var item = GetCurrentItem();

            if (item == null)
            {
                return null;
            }

            return item.MusicId;
        }

        public PlaybackQueueItem GetNextItem()
        {
            if (!HasCurrent)
            {
                return null;
            }

            var nextIndex = _currentIndex + 1;

            if (nextIndex >= _items.Count)
            {
                return null;
            }

            return _items[nextIndex];
        }

        public MusicId? GetNext()
        {
            var item = GetNextItem();

            if (item == null)
            {
                return null;
            }

            return item.MusicId;
        }

        public PlaybackQueueItem GetPreviousItem()
        {
            if (!HasCurrent)
            {
                return null;
            }

            var previousIndex = _currentIndex - 1;

            if (previousIndex < 0)
            {
                return null;
            }

            return _items[previousIndex];
        }

        public MusicId? GetPrevious()
        {
            var item = GetPreviousItem();

            if (item == null)
            {
                return null;
            }

            return item.MusicId;
        }

        public bool MoveNext()
        {
            if (!HasCurrent)
            {
                return false;
            }

            var nextIndex = _currentIndex + 1;

            if (nextIndex >= _items.Count)
            {
                return false;
            }

            _currentIndex = nextIndex;
            return true;
        }

        public bool MovePrevious()
        {
            if (!HasCurrent)
            {
                return false;
            }

            var previousIndex = _currentIndex - 1;

            if (previousIndex < 0)
            {
                return false;
            }

            _currentIndex = previousIndex;
            return true;
        }

        public bool Contains(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                return false;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].MusicId == musicId)
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            _items.Clear();
            _currentIndex = -1;
        }

        public PlaybackQueueSnapshot CreateSnapshot()
        {
            var snapshot = new PlaybackQueueSnapshot()
            {
                Items = new List<PlaybackQueueItem>(),
                CurrentIndex = _currentIndex
            };

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];

                snapshot.Items.Add(new PlaybackQueueItem
                {
                    Id = item.Id ?? string.Empty,
                    MusicId = item.MusicId,
                    Order = item.Order
                });
            }

            return snapshot;
        }

        public void Restore(PlaybackQueueSnapshot snapshot)
        {
            Clear();

            if (snapshot == null || snapshot.Items == null)
            {
                return;
            }

            foreach (var item in snapshot.Items)
            {
                if (item == null || item.MusicId.IsEmpty)
                {
                    continue;
                }

                _items.Add(new PlaybackQueueItem
                {
                    Id = item.Id ?? string.Empty,
                    MusicId = item.MusicId,
                    Order = item.Order
                });
            }

            SortAndReorder();

            if (_items.Count == 0)
            {
                _currentIndex = -1;
                return;
            }

            if (snapshot.CurrentIndex < 0)
            {
                _currentIndex = 0;
                return;
            }

            if (snapshot.CurrentIndex >= _items.Count)
            {
                _currentIndex = _items.Count - 1;
                return;
            }

            _currentIndex = snapshot.CurrentIndex;
        }

        private void SortAndReorder()
        {
            _items.Sort(CompareItems);

            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].Order = i;
            }
        }

        private static int CompareItems(PlaybackQueueItem left, PlaybackQueueItem right)
        {
            if (left == null || right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Order.CompareTo(right.Order);
        }
    }
}
