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
        private List<PlaybackQueueItem> _items = new List<PlaybackQueueItem>();

        private List<string> _shuffleItemIds = new List<string>();

        private string _currentItemId;
        
        private readonly Random _random = new Random();

        public IReadOnlyList<PlaybackQueueItem> Items
        {
            get { return _items.AsReadOnly(); }
        }

        public IReadOnlyList<string> ShuffleItemIds
        {
            get { return _shuffleItemIds.AsReadOnly(); }
        }

        public string CurrentItemId
        {
            get { return _currentItemId; }
        }

        public int CurrentIndex
        {
            get { return GetItemIndex(_currentItemId); }
        }

        public int CurrentShuffleIndex
        {
            get
            {
                if (_currentItemId == null)
                {
                    return -1;
                }

                return _shuffleItemIds.FindIndex(itemId =>string.Equals(itemId, _currentItemId, StringComparison.Ordinal));
            }
        }

        public bool HasCurrent
        {
            get { return CurrentIndex >= 0; }
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public bool HasItems
        {
            get { return _items.Count > 0; }
        }

        public void SetItems(IEnumerable<MusicId> musicIds)
        {
            if (musicIds == null)
            {
                throw new ArgumentNullException(nameof(musicIds));
            }

            var newItems = new List<PlaybackQueueItem>();

            foreach (var musicId in musicIds)
            {
                var item = PlaybackQueueItem.Create(musicId, newItems.Count);

                newItems.Add(item);
            }

            SetItems(newItems);
        }

        public void SetItems(IEnumerable<PlaybackQueueItem> items)
        {
            var copies = CloneAndValidateItems(items);

            var preparedItems = NormalizeOrder(copies);

            string currentItemId = preparedItems.Count > 0 ? preparedItems[0].Id : null;

            var shuffleItemIds = CreateShuffleOrder(preparedItems);

            ReplaceItems(preparedItems, currentItemId, shuffleItemIds);
        }

        public int Insert(IEnumerable<MusicId> musicIds, int insertIndex, int shuffleInsertIndex)
        {
            if (musicIds == null)
            {
                throw new ArgumentNullException(nameof(musicIds));
            }

            if (insertIndex < 0 || insertIndex > _items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(insertIndex));
            }

            if (shuffleInsertIndex < 0 || shuffleInsertIndex > _shuffleItemIds.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(shuffleInsertIndex));
            }

            var newItems = new List<PlaybackQueueItem>();

            foreach (var musicId in musicIds)
            {
                newItems.Add(PlaybackQueueItem.Create(musicId, newItems.Count));
            }

            if (newItems.Count == 0)
            {
                return 0;
            }

            var combinedItems = new List<PlaybackQueueItem>(_items);

            combinedItems.InsertRange(insertIndex, newItems);

            var preparedItems = CloneAndValidateItems(combinedItems);

            // 列表位置已经确定，只更新编号，不再排序。
            for (int i = 0; i < preparedItems.Count; i++)
            {
                preparedItems[i].Order = i;
            }

            var preparedShuffleItemIds = new List<string>(_shuffleItemIds);

            var newItemIds = newItems.Select(item => item.Id).ToList();

            preparedShuffleItemIds.InsertRange(shuffleInsertIndex, newItemIds);

            ReplaceItems(preparedItems, _currentItemId, preparedShuffleItemIds);

            return newItems.Count;
        }

        public int Enqueue(IEnumerable<MusicId> musicIds)
        {
            return Insert(
                musicIds,
                _items.Count,
                _shuffleItemIds.Count);
        }

        public bool SetCurrentItemId(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            int index = _items.FindIndex(item => string.Equals(item.Id, itemId, StringComparison.Ordinal));

            if (index < 0)
            {
                return false;
            }

            _currentItemId = _items[index].Id;
            return true;
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
                    _currentItemId = _items[i].Id;
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

            _currentItemId = _items[index].Id;
            return true;
        }

        public PlaybackQueueItem GetCurrentItem()
        {
            int currentIndex = CurrentIndex;

            if (currentIndex < 0)
            {
                return null;
            }

            return _items[currentIndex];
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

        public int GetItemIndex(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return -1;
            }

            return _items.FindIndex(item => string.Equals(item.Id, itemId, StringComparison.Ordinal));
        }

        public PlaybackQueueItem GetNextItem()
        {
            int currentIndex = CurrentIndex;

            if (currentIndex < 0)
            {
                return null;
            }

            int nextIndex = currentIndex + 1;

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
            int currentIndex = CurrentIndex;

            if (currentIndex < 0)
            {
                return null;
            }

            int previousIndex = currentIndex - 1;

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
            var nextItem = GetNextItem();

            if (nextItem == null)
            {
                return false;
            }

            _currentItemId = nextItem.Id;
            return true;
        }

        public bool MovePrevious()
        {
            var previousItem = GetPreviousItem();

            if (previousItem == null)
            {
                return false;
            }

            _currentItemId = previousItem.Id;
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
            _shuffleItemIds.Clear();
            _currentItemId = null;
        }


        private List<string> CreateShuffleOrder(List<PlaybackQueueItem> items)
        {
            var itemIds = new List<string>(items.Count);

            foreach (var item in items)
            {
                itemIds.Add(item.Id);
            }

            for (int i = itemIds.Count - 1; i > 0; i--)
            {
                int randomIndex = _random.Next(i + 1);

                string temporary = itemIds[i];
                itemIds[i] = itemIds[randomIndex];
                itemIds[randomIndex] = temporary;
            }

            return itemIds;
        }

        private static List<string> CloneAndValidateShuffleOrder(IEnumerable<string> shuffleItemIds, List<PlaybackQueueItem> items)
        {
            if (shuffleItemIds == null)
            {
                throw new ArgumentNullException(nameof(shuffleItemIds));
            }

            var remainingIds = new HashSet<string>(items.Select(item => item.Id), StringComparer.Ordinal);

            var copies = new List<string>(items.Count);

            foreach (string itemId in shuffleItemIds)
            {
                if (string.IsNullOrWhiteSpace(itemId) || !remainingIds.Remove(itemId))
                {
                    throw new ArgumentException("Shuffle order contains an invalid or duplicate queue item id.", nameof(shuffleItemIds));
                }

                copies.Add(itemId);
            }

            if (remainingIds.Count > 0)
            {
                throw new ArgumentException("Shuffle order must contain every queue item.", nameof(shuffleItemIds));
            }

            return copies;
        }

        public PlaybackQueueSnapshot CreateSnapshot()
        {
            var snapshot = new PlaybackQueueSnapshot()
            {
                Items = new List<PlaybackQueueItem>(),
                ShuffleItemIds = new List<string>(_shuffleItemIds),
                CurrentIndex = this.CurrentIndex
            };

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];

                snapshot.Items.Add(item.Clone());
            }

            return snapshot;
        }

        public void Restore(PlaybackQueueSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (snapshot.Items == null)
            {
                throw new ArgumentException("Snapshot items cannot be null.", nameof(snapshot));
            }

            var copies = CloneAndValidateItems(snapshot.Items);

            int savedIndex = snapshot.CurrentIndex;

            if (savedIndex < -1 || savedIndex >= copies.Count)
            {
                throw new ArgumentException("Snapshot current index is out of range.", nameof(snapshot));
            }

            string currentItemId = savedIndex >= 0 ? copies[savedIndex].Id : null;

            var preparedItems = NormalizeOrder(copies);

            ReplaceItems(preparedItems, currentItemId, snapshot.ShuffleItemIds);
        }

        private static List<PlaybackQueueItem> CloneAndValidateItems(IEnumerable<PlaybackQueueItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var copies = new List<PlaybackQueueItem>();

            var itemIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var item in items)
            {
                if (item == null)
                {
                    throw new ArgumentException("Queue items cannot contain null.", nameof(items));
                }

                var copy = item.Clone();

                if (!itemIds.Add(copy.Id))
                {
                    throw new ArgumentException($"Duplicate queue item id: {copy.Id}", nameof(items));
                }

                copies.Add(copy);
            }

            return copies;
        }

        private static List<PlaybackQueueItem> NormalizeOrder(List<PlaybackQueueItem> items)
        {
            var orderedItems = items.OrderBy(item => item.Order).ToList();

            for (int i = 0; i < orderedItems.Count; i++)
            {
                orderedItems[i].Order = i;
            }

            return orderedItems;
        }

        private void ReplaceItems(List<PlaybackQueueItem> preparedItems, string currentItemId, IEnumerable<string> shuffleItemIds)
        {
            if (currentItemId != null)
            {
                bool containsCurrentItem = preparedItems.Exists(item =>string.Equals(item.Id, currentItemId, StringComparison.Ordinal));

                if (!containsCurrentItem)
                {
                    throw new ArgumentException("Current item must exist in the prepared queue.", nameof(currentItemId));
                }
            }

            var preparedShuffleItemIds = CloneAndValidateShuffleOrder(shuffleItemIds, preparedItems);

            _items = preparedItems;
            _shuffleItemIds = preparedShuffleItemIds;
            _currentItemId = currentItemId;
        }
    }
}
