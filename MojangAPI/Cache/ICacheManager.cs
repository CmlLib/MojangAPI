namespace MojangAPI.Cache
{
    public interface ICacheManager<T>
    {
        T GetDefaultObject();
        T ReadCache();
        void SaveCache(T obj);
    }
}
