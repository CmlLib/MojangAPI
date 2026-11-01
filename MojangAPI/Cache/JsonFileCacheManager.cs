using MojangAPI.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.IO;
using System.Text.Json;

namespace MojangAPI.Cache
{
    public class JsonFileCacheManager<T> : ICacheManager<T> where T : new()
    {
        private readonly JsonTypeInfo<T>? typeInfo;

        public string CacheFilePath { get; private set; }

        public JsonFileCacheManager(string filepath) : this(filepath, null)
        {
        }

        public JsonFileCacheManager(string filepath, JsonTypeInfo<T>? typeInfo)
        {
            this.typeInfo = typeInfo;
            this.CacheFilePath = filepath;
        }

        public virtual T GetDefaultObject() => new T();

        public T ReadCache()
        {
            if (!File.Exists(CacheFilePath))
                return GetDefaultObject();

            try
            {
                string filecontent = File.ReadAllText(CacheFilePath);
                return MojangJson.Deserialize<T>(filecontent, typeInfo) ?? new T();
            }
            catch
            {
                return GetDefaultObject();
            }
        }

        public void SaveCache(T obj)
        {
            var dirPath = Path.GetDirectoryName(CacheFilePath);
            if (!string.IsNullOrEmpty(dirPath))
                Directory.CreateDirectory(dirPath);
            File.WriteAllText(CacheFilePath, MojangJson.Serialize(obj, typeInfo));
        }
    }
}
