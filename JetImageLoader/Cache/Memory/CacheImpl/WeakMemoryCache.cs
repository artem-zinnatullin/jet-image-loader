
namespace JetImageLoader.Cache.Memory.CacheImpl
{
    public class WeakMemoryCache<TKey, TValue> : BaseMemoryCache<TKey, TValue> where TKey : class where TValue : class
    {
        private readonly SynchronizedWeakDictionary<TKey, TValue> _synchronizedWeakDictionary = new SynchronizedWeakDictionary<TKey, TValue>(); 

        public override TValue Get(TKey key)
        {
            return _synchronizedWeakDictionary[key];
        }

        public override void Put(TKey key, TValue value)
        {
            _synchronizedWeakDictionary.Add(key, value);
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
