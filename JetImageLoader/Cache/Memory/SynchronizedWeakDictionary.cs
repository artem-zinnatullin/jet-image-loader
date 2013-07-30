
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace JetImageLoader.Cache.Memory
{
    public class SynchronizedWeakDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : class where TValue : class
    {
        private readonly ConditionalWeakTable<TKey, TValue> _weakTable = new ConditionalWeakTable<TKey, TValue>();
        
        private readonly object _lockObj = new object();

        private readonly IList<TKey> _keyList = new List<TKey>(); 

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (_lockObj)
            {
                _weakTable.Add(item.Key, item.Value);
                _keyList.Add(item.Key);
                Count++;
            }
        }

        public void Clear()
        {
            lock (_lockObj)
            {
                foreach (var key in _keyList)
                {
                    _weakTable.Remove(key);
                }

                _keyList.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (_lockObj)
            {
                TValue o;
                return _weakTable.TryGetValue(item.Key, out o);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; private set; }

        public bool IsReadOnly { get; private set; }
        public void Add(TKey key, TValue value)
        {
            lock (_lockObj)
            {
                _weakTable.Add(key, value);
                _keyList.Add(key);
                Count++;
            }
        }

        public bool ContainsKey(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TKey key)
        {
            lock (_lockObj)
            {
                if (_keyList.Remove(key))
                {
                    Count--;
                    _weakTable.Remove(key);
                    return true;
                }

                return false;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_lockObj)
            {
                return _weakTable.TryGetValue(key, out value);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (_lockObj)
                {
                    TValue value;
                    return _weakTable.TryGetValue(key, out value) ? value : null;
                }
            }

            set
            {
                lock (_lockObj)
                {
                    _weakTable.Remove(key);
                    _weakTable.Add(key, value);
                }
            }
        }

        public ICollection<TKey> Keys 
        { 
            get
            {
                lock (_lockObj)
                {
                    return new List<TKey>(_keyList);
                }
            } 
        }

        public ICollection<TValue> Values { get; private set; }
    }
}
