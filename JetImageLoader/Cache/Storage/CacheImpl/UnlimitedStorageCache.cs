
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
        /// <summary>
        /// Creates instance 
        /// </summary>
        /// <param name="isf">IsolatedStorageFile instance to work with file system</param>
        /// <param name="cacheDirectory">Directory to store cache, starting with two slashes "\\"</param>
        /// <param name="cacheFileNameGenerator">ICacheFileNameGenerator instance to generate cache filenames</param>
        /// <param name="cacheMaxLifetimeInMillis">Cache max lifetime in millis, for example two weeks = 2 * 7 * 24 * 60 * 60 * 1000; default value == 0; pass value &lt;= 0 to disable max cache lifetime</param>
        public UnlimitedStorageCache(IsolatedStorageFile isf, string cacheDirectory, ICacheFileNameGenerator cacheFileNameGenerator, long cacheMaxLifetimeInMillis = 0)
            : base(isf, cacheDirectory, cacheFileNameGenerator, cacheMaxLifetimeInMillis)
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
