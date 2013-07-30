
using System;
using System.Security.Cryptography;
using System.Text;

namespace JetImageLoader.Cache.Storage.CacheFileNameGenerators
{
    /// <summary>
    /// Using SHA1 hash generator to generate cache file names
    /// </summary>
    public class SHA1CacheFileNameGenerator : ICacheFileNameGenerator
    {
        public string GenerateCacheFileName(string fileUrl)
        {
            return SHA1Helper.ComputeHash(fileUrl);
        }

        private static class SHA1Helper
        {
            private static readonly SHA1Managed Sha1Managed = new SHA1Managed();
            private static readonly UTF8Encoding Utf8Encoding = new UTF8Encoding();

            /// <summary>
            /// Computes SHA1 hash for the source string
            /// SHA1 because there is no .NET implementation of MD5 in WP .NET platform :(
            /// </summary>
            /// <param name="source">Source string to compute hash from</param>
            /// <returns>SHA1 hash from the source string</returns>
            public static string ComputeHash(string source)
            {
                var hash = Sha1Managed.ComputeHash(Utf8Encoding.GetBytes(source.ToCharArray()));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}
