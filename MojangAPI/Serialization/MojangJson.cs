using System;
using System.Text.Json.Serialization.Metadata;

namespace MojangAPI.Serialization
{
    internal static class MojangJson
    {
        internal static JsonTypeInfo<T> GetTypeInfo<T>()
        {
            // Callers must supply source-generated metadata for custom types.
            var typeInfo = MojangJsonContext.Default.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>;
            if (typeInfo == null)
                throw new NotSupportedException($"Supply a source-generated JsonTypeInfo<{typeof(T).Name}> for custom JSON types.");

            return typeInfo;
        }
    }
}
