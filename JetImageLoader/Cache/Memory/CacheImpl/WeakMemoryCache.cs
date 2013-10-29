
using System;

namespace JetImageLoader.Cache.Memory.CacheImpl
{
    public class WeakMemoryCache<TKey, TValue> : BaseMemoryCache<TKey, TValue> where TKey : class where TValue : class
    {
        private readonly SynchronizedWeakRefDictionary<TKey, TValue> _synchronizedWeakDictionary = new SynchronizedWeakRefDictionary<TKey, TValue>(); 

        public override TValue Get(TKey key)
        {
            return _synchronizedWeakDictionary[key];
        }

        public override void Put(TKey key, TValue value)
        {
            try
            {
                _synchronizedWeakDictionary.Add(key, value);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                // Already exists in weak, do nothing                
            }
        }

        public override bool TryGetValue(TKey key, out TValue value)
        {
            return _synchronizedWeakDictionary.TryGetValue(key, out value);
        }

        public override void Clear()
        {
            _synchronizedWeakDictionary.Clear();
        }
    }
}
