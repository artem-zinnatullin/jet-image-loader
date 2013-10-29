
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace JetImageLoader.Cache.Storage
{
    public abstract class BaseStorageCache
    {
        /// <summary>
        /// Default value of cache max lifetime in milliseconds
        /// Equals to one week 604800000 milliseconds == 7 * 24 * 60 * 60 * 1000
        /// </summary>
        protected const long DefaultCacheMaxLifetimeInMillis = 7 * 24 * 60 * 60 * 1000; // == 604800000;

        /// <summary>
        /// IsolatedStorageFile instance to work with app's ISF
        /// </summary>
        protected readonly IsolatedStorageFile ISF;

        /// <summary>
        /// Base cache directory where all cache will be saved
        /// </summary>
        protected virtual string CacheDirectory { get; set; }

        /// <summary>
        /// Generates file name from the cache key
        /// </summary>
        protected virtual ICacheFileNameGenerator CacheFileNameGenerator { get; set; }

        /// <summary>
        /// Maximum age of cache in milliseconds
        /// <= 0 — always alive
        /// </summary>
        protected virtual long CacheMaxLifetimeInMillis { get; set; }

        protected BaseStorageCache(IsolatedStorageFile isf, string cacheDirectory, ICacheFileNameGenerator cacheFileNameGenerator, long cacheMaxLifetimeInMillis = DefaultCacheMaxLifetimeInMillis)
        {
            if (isf == null)
            {
                throw new ArgumentNullException("isf");
            }

            if (String.IsNullOrEmpty(cacheDirectory))
            {
                throw new ArgumentException("cacheDirectory name could not be null or empty");
            }

            if (!cacheDirectory.StartsWith("\\"))
            {
                throw new ArgumentException("cacheDirectory name should starts with double slashes: \\");
            }

            if (cacheFileNameGenerator == null)
            {
                throw new ArgumentNullException("cacheFileNameGenerator");
            }

            ISF = isf;
            CacheDirectory = cacheDirectory;
            CacheFileNameGenerator = cacheFileNameGenerator;
            CacheMaxLifetimeInMillis = cacheMaxLifetimeInMillis;

            // Creating cache directory if it not exists
            ISF.CreateDirectory(CacheDirectory);
        }

        /// <summary>
        /// You should implement this method. Usefull to handle cache saving as you want
        /// Base implementation is InternalSaveAsync(), you can call it in your implementation
        /// </summary>
        /// <param name="cacheKey">will be used by CacheFileNameGenerator</param>
        /// <param name="cacheStream">will be written to the cache file</param>
        /// <returns>true if cache was saved, false otherwise</returns>
        public abstract Task<bool> SaveAsync(string cacheKey, Stream cacheStream);

        /// <summary>
        /// Saves the file with fullFilePath, uses FileMode.Create, so file create time will be rewrited if needed
        /// If exception has occurred while writing the file, it will delete it
        /// </summary>
        /// <param name="fullFilePath">example: "\\image_cache\\213898adj0jd0asd</param>
        /// <param name="cacheStream">stream to write to the file</param>
        /// <returns>true if file was successfully written, false otherwise</returns>
        protected async virtual Task<bool> InternalSaveAsync(string fullFilePath, Stream cacheStream)
        {
            using (var fileStream = new IsolatedStorageFileStream(fullFilePath, FileMode.Create, FileAccess.ReadWrite, ISF))
            {
                try
                {
                    await cacheStream.CopyToAsync(fileStream);
                    return true;
                }
                catch
                {
                    try
                    {
                        // If file was not saved normally, we should delete it
                        ISF.DeleteFile(fullFilePath);
                    }
                    catch
                    {
                        JetImageLoader.Log("[error] can not delete unsaved file: " + fullFilePath);
                    }
                }
            }

            JetImageLoader.Log("[error] can not save cache to the: " + fullFilePath);
            return false;
        }

        /// <summary>
        /// Async gets file stream by the cacheKey (cacheKey will be converted using CacheFileNameGenerator)
        /// </summary>
        /// <param name="cacheKey">key will be used by CacheFileNameGenerator to get cache's file name</param>
        /// <returns>Stream of that file or null, if it does not exists</returns>
        public async virtual Task<Stream> LoadCacheStreamAsync(string cacheKey)
        {
            var fullFilePath = GetFullFilePath(CacheFileNameGenerator.GenerateCacheFileName(cacheKey));

            if (!ISF.FileExists(fullFilePath)) return null;
            
            try
            {
                var cacheFileMemoryStream = new MemoryStream();

                using (var cacheFileStream = ISF.OpenFile(fullFilePath, FileMode.Open, FileAccess.Read))
                {
                    await cacheFileStream.CopyToAsync(cacheFileMemoryStream);
                    return cacheFileMemoryStream;
                }
            }
            catch
            {
                JetImageLoader.Log("[error] can not load file stream from: " + fullFilePath);
                return null;
            }
        }

        /// <summary>
        /// Gets full file path, combining it with CacheDirectory
        /// </summary>
        /// <param name="fileName">name of the file</param>
        /// <returns>full path to the file</returns>
        protected virtual string GetFullFilePath(string fileName)
        {
            return Path.Combine(CacheDirectory, fileName);
        }

        /// <summary>
        /// Checks file existence
        /// </summary>
        /// <param name="cacheKey">Will be used by CacheFileNameGenerator</param>
        /// <returns>true if file with cache exists, false otherwise</returns>
        public virtual bool IsCacheExists(string cacheKey)
        {
            var fullFilePath = GetFullFilePath(CacheFileNameGenerator.GenerateCacheFileName(cacheKey));

            try
            {
                return ISF.FileExists(fullFilePath);
            }
            catch
            {
                JetImageLoader.Log("[error] can not check cache existence, file: " + fullFilePath);
                return false;
            }
        }

        /// <summary>
        /// Checks is cache existst and its last write time &lt;= CacheMaxLifetimeInMillis
        /// </summary>
        /// <param name="cacheKey">Will be used by CacheFileNameGenerator</param>
        /// <returns>true if cache exists and alive, false otherwise</returns>
        public virtual bool IsCacheExistsAndAlive(string cacheKey)
        {
            var fullFilePath = GetFullFilePath(CacheFileNameGenerator.GenerateCacheFileName(cacheKey));

            try
            {
                if (ISF.FileExists(fullFilePath))
                {
                    return CacheMaxLifetimeInMillis <= 0 ? true : ((DateTime.Now - ISF.GetLastWriteTime(fullFilePath)).TotalMilliseconds < CacheMaxLifetimeInMillis);
                }
            }
            catch
            {
                JetImageLoader.Log("[error] can not check is cache exists and alive, file: " + fullFilePath);
            }

            return false;
        }

        /// <summary>
        /// Deletes all cache from CacheDirectory
        /// </summary>
        public virtual void Clear()
        {
            DeleteDirContent(CacheDirectory);
        }

        /// <summary>
        /// Recursive method to delete all content of needed directory
        /// </summary>
        /// <param name="absoluteDirPath">Path of the dir, which content you want to delete</param>
        protected virtual void DeleteDirContent(string absoluteDirPath)
        {
            // Pattern to match all innerFiles and innerDirectories inside absoluteDirPath
            var filesAndDirectoriesPattern = absoluteDirPath + @"\*";

            string[] innerFiles;
            try
            {
                innerFiles = ISF.GetFileNames(filesAndDirectoriesPattern);
            }
            catch
            {
                // throw new Exception(String.Format("Can not get array of innerFiles in innerDirectory {0}, exception occurred: {1}", absoluteDirPath, e.Message));
                return;
            }

            // Deleting all innerFiles in current innerDirectory
            foreach (var innerFile in innerFiles)
            {
                try
                {
                    ISF.DeleteFile(Path.Combine(absoluteDirPath, innerFile));
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                    // throw new Exception(String.Format("Can not delete file {0}, exception occurred: {1}", innerFile, e.Message));
                }
            }

            string[] innerDirectories;
            try
            {
                innerDirectories = ISF.GetDirectoryNames(filesAndDirectoriesPattern);
            }
            catch
            {
                // throw new Exception(String.Format("Can not get array of innerDirectories in innerDirectory {0}, exception occurred: {1}", absoluteDirPath, e.Message));
                return;
            }

            // Recursively deleting content of each innerDirectory
            foreach (var innerDirectory in innerDirectories)
            {
                DeleteDirContent(Path.Combine(absoluteDirPath, innerDirectory));
            }
        }
    }
}
