
using JetImageLoader.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JetImageLoader.Cache.Storage.CacheImpl
{
    public class LimitedStorageCache : BaseStorageCache
    {
        /// <summary>
        /// Dictionary contains pairs of filePath and last access time in unix timestamp * 1000 (DateTime.Millisecond)
        /// </summary>
        private readonly IDictionary<string, long> _lastAccessTimeDictionary = new SynchronizedDictionary<string, long>();

        private readonly object _lockObject = new object();

        private long _currentCacheSizeInBytes = -1;

        protected long CurrentCacheSizeInBytes
        {
            get
            {
                lock (_lockObject)
                {
                    return _currentCacheSizeInBytes;
                }
            }

            set
            {
                lock (_lockObject)
                {
                    _currentCacheSizeInBytes = value;
                }
            }
        }

        private readonly long _cacheLimitInBytes;

        /// <summary>
        /// Creates new LimitedStorageCache instance
        /// </summary>
        /// <param name="isf">IsolatedStorageFile instance to work with file system</param>
        /// <param name="cacheDirectory">Directory to store cache, starting with two slashes "\\"</param>
        /// <param name="cacheFileNameGenerator">ICacheFileNameGenerator instance to generate cache filenames</param>
        /// <param name="cacheLimitInBytes">Limit of total cache size in bytes, for example 10 mb == 10 * 1024 * 1024</param>
        /// <param name="cacheMaxLifetimeInMillis">Cache max lifetime in millis, for example two weeks = 2 * 7 * 24 * 60 * 60 * 1000; default value == one week; pass value &lt;= 0 to disable max cache lifetime</param>
        public LimitedStorageCache(IsolatedStorageFile isf, string cacheDirectory, ICacheFileNameGenerator cacheFileNameGenerator, long cacheLimitInBytes, long cacheMaxLifetimeInMillis = DefaultCacheMaxLifetimeInMillis)
            : base(isf, cacheDirectory, cacheFileNameGenerator, cacheMaxLifetimeInMillis)
        {
            _cacheLimitInBytes = cacheLimitInBytes;
            BeginCountCurrentCacheSize();
        }

        public async override Task<bool> SaveAsync(string cacheKey, Stream cacheStream)
        {
            var fullFileName = Path.Combine(CacheDirectory, CacheFileNameGenerator.GenerateCacheFileName(cacheKey));
            var cacheSizeInBytes = cacheStream.Length;

            while (CurrentCacheSizeInBytes + cacheSizeInBytes > _cacheLimitInBytes)
            {
                if (!RemoveOldestCacheFile())
                {
                    break; // All cache deleted
                }
            }

            var wasFileSaved = await base.InternalSaveAsync(fullFileName, cacheStream);
            
            if (wasFileSaved)
            {
                _lastAccessTimeDictionary[Path.Combine(CacheDirectory, fullFileName)] = DateTimeUtil.CurrentTimeMillis();
                CurrentCacheSizeInBytes += cacheStream.Length; // Updating current cache size
            }

            return wasFileSaved;
        }

        private void BeginCountCurrentCacheSize()
        {
            Task.Factory.StartNew(() =>
            {
                // Pattern to match all innerFiles and innerDirectories inside absoluteDirPath
                var filesAndDirectoriesPattern = CacheDirectory + @"\*";

                string[] cacheFileNames;

                try
                {
                    cacheFileNames = ISF.GetFileNames(Path.Combine(CacheDirectory, filesAndDirectoriesPattern));
                }
                catch
                {
                    return;
                }

                long cacheSizeInBytes = 0;

                foreach (var cacheFileName in cacheFileNames)
                {
                    var fullCacheFilePath = Path.Combine(CacheDirectory, cacheFileName);

                    try
                    {
                        using (var file = ISF.OpenFile(fullCacheFilePath, FileMode.Open, FileAccess.Read))
                        {
                            cacheSizeInBytes += file.Length;

                            _lastAccessTimeDictionary.Add(fullCacheFilePath, DateTimeUtil.ConvertDateTimeToMillis(ISF.GetLastAccessTime(fullCacheFilePath).DateTime));
                        }
                    }
                    catch
                    {
                        JetImageLoader.Log("[error] can not get cache's file size: " + fullCacheFilePath);
                    }
                }

                CurrentCacheSizeInBytes += cacheSizeInBytes; // Updating current cache size
            });
        }

        /// <summary>
        /// Removing oldest cache file (file, which last access time is smaller)
        /// </summary>
        private bool RemoveOldestCacheFile()
        {
            if (_lastAccessTimeDictionary.Count == 0) return false;

            var oldestCacheFilePath = _lastAccessTimeDictionary.Aggregate((pair1, pair2) => (pair1.Value < pair2.Value)? pair1 : pair2).Key;

            if (String.IsNullOrEmpty(oldestCacheFilePath)) return false;

            try
            {
                long fileSizeInBytes;

                using (var file = ISF.OpenFile(oldestCacheFilePath, FileMode.Open, FileAccess.Read))
                {
                    fileSizeInBytes = file.Length;
                }

                try
                {
                    ISF.DeleteFile(oldestCacheFilePath);
                    _lastAccessTimeDictionary.Remove(oldestCacheFilePath);
                    CurrentCacheSizeInBytes -= fileSizeInBytes; // Updating current cache size

                    JetImageLoader.Log("[delete] cache file " + oldestCacheFilePath);
                    return true;
                }
                catch
                {
                    JetImageLoader.Log("[error] can not delete oldest cache file: " + oldestCacheFilePath);
                }
            }
            catch
            {
                JetImageLoader.Log("[error] can not get olders cache's file size: " + oldestCacheFilePath);
            }

            return false;
        }
    }
}
