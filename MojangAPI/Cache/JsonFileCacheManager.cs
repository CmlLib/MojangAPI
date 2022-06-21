using System.IO;
using System.Text.Json;

namespace MojangAPI.Cache
{
    public class JsonFileCacheManager<T> : ICacheManager<T>
    {
        public string CacheFilePath { get; private set; }

        public JsonFileCacheManager(string filepath)
        {
            this.CacheFilePath = filepath;
        }

        public virtual T GetDefaultObject() => default(T);

        public T ReadCache()
        {
            if (!File.Exists(CacheFilePath))
                return GetDefaultObject();

            try
            {
                string filecontent = File.ReadAllText(CacheFilePath);
                return JsonSerializer.Deserialize<T>(filecontent);
            }
            catch
            {
                return GetDefaultObject();
            }
        }

        public void SaveCache(T obj)
        {
            try
            {
                var dirPath = Path.GetDirectoryName(CacheFilePath);
                if (!string.IsNullOrEmpty(dirPath))
                    Directory.CreateDirectory(dirPath);
                File.WriteAllText(CacheFilePath, JsonSerializer.Serialize(obj));
            }
            catch
            {

            }
        }
    }
}
