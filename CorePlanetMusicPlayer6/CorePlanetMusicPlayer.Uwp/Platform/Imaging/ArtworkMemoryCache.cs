using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Uwp.Platform.Imaging
{
    internal sealed class ArtworkMemoryCache
    {
        private const long MaximumBytes = 32L * 1024 * 1024;
        private const int MaximumItems = 256;

        private readonly Dictionary<string, LinkedListNode<Entry>> _entries = new Dictionary<string, LinkedListNode<Entry>>(StringComparer.Ordinal);

        private readonly LinkedList<Entry> _recent = new LinkedList<Entry>();

        private long _estimatedBytes;

        private sealed class Entry
        {
            public string Key;
            public string OwnerKey;
            public BitmapImage Image;
            public long EstimatedBytes;
            public DateTimeOffset ExpiresAt;
        }

        public int Count
        {
            get { return _entries.Count; }
        }

        public long EstimatedBytes
        {
            get { return _estimatedBytes; }
        }

        public bool TryGet(string key, out BitmapImage image)
        {
            image = null;

            LinkedListNode<Entry> node;

            if (!_entries.TryGetValue(key, out node))
            {
                return false;
            }

            if (node.Value.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                Remove(node);
                return false;
            }

            // 最近使用的放在链表头部。
            _recent.Remove(node);
            _recent.AddFirst(node);

            image = node.Value.Image;
            return true;
        }

        public void Set(string key, string ownerKey, BitmapImage image, TimeSpan lifetime)
        {
            LinkedListNode<Entry> existing;

            if (_entries.TryGetValue(key, out existing))
            {
                Remove(existing);
            }

            long estimatedBytes = 0;

            if (image != null)
            {
                if (image.PixelWidth <= 0 || image.PixelHeight <= 0)
                {
                    return;
                }

                estimatedBytes = (long)image.PixelWidth * image.PixelHeight * 4;

                if (estimatedBytes > MaximumBytes)
                {
                    return;
                }
            }

            var entry = new Entry
            {
                Key = key,
                OwnerKey = ownerKey,
                Image = image,
                EstimatedBytes = estimatedBytes,
                ExpiresAt = DateTimeOffset.UtcNow.Add(lifetime)
            };

            var node = _recent.AddFirst(entry);

            _entries.Add(key, node);
            _estimatedBytes += estimatedBytes;

            while (_entries.Count > MaximumItems || _estimatedBytes > MaximumBytes)
            {
                Remove(_recent.Last);
            }
        }

        public void RemoveOwner(string ownerKey)
        {
            foreach (var node in _entries.Values.ToList())
            {
                if (string.Equals(node.Value.OwnerKey, ownerKey, StringComparison.Ordinal))
                {
                    Remove(node);
                }
            }
        }

        public void Clear()
        {
            _entries.Clear();
            _recent.Clear();
            _estimatedBytes = 0;
        }

        private void Remove(LinkedListNode<Entry> node)
        {
            if (node == null)
            {
                return;
            }

            _entries.Remove(node.Value.Key);
            _recent.Remove(node);
            _estimatedBytes -= node.Value.EstimatedBytes;
        }
    }
}
