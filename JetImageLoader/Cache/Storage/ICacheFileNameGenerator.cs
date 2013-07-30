
namespace JetImageLoader.Cache.Storage
{
    public interface ICacheFileNameGenerator
    {
        string GenerateCacheFileName(string fileUrl);
    }
}
