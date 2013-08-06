
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace JetImageLoader.Cache.Storage.CacheImpl
{
    /// <summary>
    /// Simpliest implemetation of BaseStorageCache
    /// Unlimited storage cache, it will never delete old cache files
    /// </summary>
    public class UnlimitedStorageCache : BaseStorageCache
    {
        public UnlimitedStorageCache(IsolatedStorageFile isf, string cacheDirectory, ICacheFileNameGenerator cacheFileNameGenerator) : base(isf, cacheDirectory, cacheFileNameGenerator)
        {
        }

        /// <summary>
        /// Just calls BaseStorageCache.InternalSaveAsync() without any other operation
        /// </summary>
        /// <param name="cacheKey">will be used by CacheFileNameGenerator</param>
        /// <param name="cacheStream">will be written to the cache file</param>
        /// <returns>true if cache was saved, false otherwise</returns>
        public override Task<bool> SaveAsync(string cacheKey, Stream cacheStream)
        {
            return InternalSaveAsync(CacheFileNameGenerator.GenerateCacheFileName(cacheKey), cacheStream);
        }
    }   
}
