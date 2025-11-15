//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CorePlanetMusicPlayer.App
//{
//    /// <summary>
//    /// 具有变更通知功能的字典集合
//    /// </summary>
//    /// <typeparam name="TKey">键类型</typeparam>
//    /// <typeparam name="TValue">值类型</typeparam>
//    public class ObservableDictionary<TKey, TValue> :
//        IDictionary<TKey, TValue>,
//        INotifyCollectionChanged,
//        INotifyPropertyChanged,
//        IReadOnlyDictionary<TKey, TValue>
//    {
//        private readonly Dictionary<TKey, TValue> _dictionary;
//        private const string CountString = "Count";
//        private const string IndexerName = "Item[]";

//        /// <summary>
//        /// 初始化 ObservableDictionary 类的新实例
//        /// </summary>
//        public ObservableDictionary()
//        {
//            _dictionary = new Dictionary<TKey, TValue>();
//        }

//        /// <summary>
//        /// 使用指定的比较器初始化 ObservableDictionary 类的新实例
//        /// </summary>
//        public ObservableDictionary(IEqualityComparer<TKey> comparer)
//        {
//            _dictionary = new Dictionary<TKey, TValue>(comparer);
//        }

//        /// <summary>
//        /// 从现有字典初始化 ObservableDictionary 类的新实例
//        /// </summary>
//        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
//        {
//            _dictionary = new Dictionary<TKey, TValue>(dictionary);
//        }

//        /// <summary>
//        /// 当集合更改时发生
//        /// </summary>
//        public event NotifyCollectionChangedEventHandler CollectionChanged;

//        /// <summary>
//        /// 当属性值更改时发生
//        /// </summary>
//        public event PropertyChangedEventHandler PropertyChanged;

//        /// <summary>
//        /// 获取包含字典中的键的集合
//        /// </summary>
//        public ICollection<TKey> Keys => _dictionary.Keys;

//        /// <summary>
//        /// 获取包含字典中的值的集合
//        /// </summary>
//        public ICollection<TValue> Values => _dictionary.Values;

//        /// <summary>
//        /// 获取字典中的键的只读集合
//        /// </summary>
//        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

//        /// <summary>
//        /// 获取字典中的值的只读集合
//        /// </summary>
//        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

//        /// <summary>
//        /// 获取字典中包含的键/值对的数目
//        /// </summary>
//        public int Count => _dictionary.Count;

//        /// <summary>
//        /// 获取一个值，该值指示字典是否为只读
//        /// </summary>
//        public bool IsReadOnly => false;

//        /// <summary>
//        /// 获取或设置与指定的键相关联的值
//        /// </summary>
//        /// <param name="key">要获取或设置的值的键</param>
//        /// <returns>与指定的键相关联的值</returns>
//        public TValue this[TKey key]
//        {
//            get => _dictionary[key];
//            set
//            {
//                if (ContainsKey(key))
//                {
//                    var oldValue = _dictionary[key];
//                    _dictionary[key] = value;
//                    OnCollectionChanged(NotifyCollectionChangedAction.Replace,
//                                       new KeyValuePair<TKey, TValue>(key, oldValue),
//                                       new KeyValuePair<TKey, TValue>(key, value));
//                    OnPropertyChanged(IndexerName);
//                }
//                else
//                {
//                    Add(key, value);
//                }
//            }
//        }

//        /// <summary>
//        /// 将指定的键和值添加到字典中
//        /// </summary>
//        /// <param name="key">要添加的元素的键</param>
//        /// <param name="value">要添加的元素的值</param>
//        public void Add(TKey key, TValue value)
//        {
//            if (key == null) throw new ArgumentNullException(nameof(key));

//            _dictionary.Add(key, value);
//            OnCollectionChanged(NotifyCollectionChangedAction.Add,
//                               new KeyValuePair<TKey, TValue>(key, value));
//        }

//        /// <summary>
//        /// 将指定的键/值对添加到字典中
//        /// </summary>
//        /// <param name="item">要添加到字典中的键/值对</param>
//        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

//        /// <summary>
//        /// 从字典中移除所有键/值对
//        /// </summary>
//        public void Clear()
//        {
//            if (_dictionary.Count > 0)
//            {
//                _dictionary.Clear();
//                OnCollectionChanged();
//            }
//        }

//        /// <summary>
//        /// 确定字典是否包含特定的键/值对
//        /// </summary>
//        /// <param name="item">要在字典中查找的键/值对</param>
//        /// <returns>如果在字典中找到 item，则为 true；否则为 false</returns>
//        public bool Contains(KeyValuePair<TKey, TValue> item) =>
//            _dictionary.TryGetValue(item.Key, out var value) &&
//            EqualityComparer<TValue>.Default.Equals(value, item.Value);

//        /// <summary>
//        /// 确定字典是否包含指定的键
//        /// </summary>
//        /// <param name="key">要在字典中查找的键</param>
//        /// <returns>如果字典包含具有指定键的元素，则为 true；否则为 false</returns>
//        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

//        /// <summary>
//        /// 从特定的 Array 索引开始，将字典的元素复制到一个 Array 中
//        /// </summary>
//        /// <param name="array">作为从字典复制的元素的目标的一维 Array</param>
//        /// <param name="arrayIndex">array 中从零开始的索引，从此处开始复制</param>
//        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
//        {
//            if (array == null) throw new ArgumentNullException(nameof(array));
//            if (arrayIndex < 0 || arrayIndex > array.Length)
//                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
//            if (array.Length - arrayIndex < Count)
//                throw new ArgumentException("目标数组容量不足");

//            int index = arrayIndex;
//            foreach (var item in _dictionary)
//            {
//                array[index++] = item;
//            }
//        }

//        /// <summary>
//        /// 从字典中移除带有指定键的元素
//        /// </summary>
//        /// <param name="key">要移除的元素的键</param>
//        /// <returns>如果该元素已成功移除，则为 true；否则为 false</returns>
//        public bool Remove(TKey key)
//        {
//            if (key == null) throw new ArgumentNullException(nameof(key));

//            if (_dictionary.TryGetValue(key, out var value) && _dictionary.Remove(key))
//            {
//                OnCollectionChanged(NotifyCollectionChangedAction.Remove,
//                                   new KeyValuePair<TKey, TValue>(key, value));
//                return true;
//            }
//            return false;
//        }

//        /// <summary>
//        /// 从字典中移除特定的键/值对
//        /// </summary>
//        /// <param name="item">要从字典中移除的键/值对</param>
//        /// <returns>如果该元素已成功移除，则为 true；否则为 false</returns>
//        public bool Remove(KeyValuePair<TKey, TValue> item)
//        {
//            if (Contains(item) && _dictionary.Remove(item.Key))
//            {
//                OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
//                return true;
//            }
//            return false;
//        }

//        /// <summary>
//        /// 获取与指定键关联的值
//        /// </summary>
//        /// <param name="key">要获取其值的键</param>
//        /// <param name="value">当此方法返回时，如果找到指定键，则返回与该键相关联的值；否则，返回 value 参数的类型默认值</param>
//        /// <returns>如果字典包含具有指定键的元素，则为 true；否则为 false</returns>
//        public bool TryGetValue(TKey key, out TValue value) =>
//            _dictionary.TryGetValue(key, out value);

//        /// <summary>
//        /// 返回循环访问字典的枚举数
//        /// </summary>
//        /// <returns>用于字典的枚举数</returns>
//        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
//            _dictionary.GetEnumerator();

//        /// <summary>
//        /// 返回循环访问集合的枚举数
//        /// </summary>
//        /// <returns>用于集合的枚举数</returns>
//        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

//        /// <summary>
//        /// 引发 CollectionChanged 事件
//        /// </summary>
//        private void OnCollectionChanged()
//        {
//            OnPropertyChanged(CountString);
//            OnPropertyChanged(IndexerName);
//            CollectionChanged?.Invoke(this,
//                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
//        }

//        /// <summary>
//        /// 引发 CollectionChanged 事件
//        /// </summary>
//        private void OnCollectionChanged(NotifyCollectionChangedAction action,
//                                        KeyValuePair<TKey, TValue> changedItem)
//        {
//            OnPropertyChanged(CountString);
//            OnPropertyChanged(IndexerName);
//            CollectionChanged?.Invoke(this,
//                new NotifyCollectionChangedEventArgs(action, changedItem));
//        }

//        /// <summary>
//        /// 引发 CollectionChanged 事件
//        /// </summary>
//        private void OnCollectionChanged(NotifyCollectionChangedAction action,
//                                        KeyValuePair<TKey, TValue> oldItem,
//                                        KeyValuePair<TKey, TValue> newItem)
//        {
//            OnPropertyChanged(CountString);
//            OnPropertyChanged(IndexerName);
//            CollectionChanged?.Invoke(this,
//                new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
//        }

//        /// <summary>
//        /// 引发 PropertyChanged 事件
//        /// </summary>
//        private void OnPropertyChanged(string propertyName)
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }
//    }
//}
