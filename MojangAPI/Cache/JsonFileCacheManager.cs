﻿using System.IO;
using System.Text.Json;

namespace MojangAPI.Cache
{
    public class JsonFileCacheManager<T> : ICacheManager<T> where T : new()
    {
        public string CacheFilePath { get; private set; }

        public JsonFileCacheManager(string filepath)
        {
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
                return JsonSerializer.Deserialize<T>(filecontent) ?? new T();
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
            File.WriteAllText(CacheFilePath, JsonSerializer.Serialize(obj));
        }
    }
}
