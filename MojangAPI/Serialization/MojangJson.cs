using System;
using System.Text.Json.Serialization.Metadata;

namespace MojangAPI.Serialization
{
    internal static class MojangJson
    {
        internal static JsonTypeInfo<T> GetTypeInfo<T>()
        {
            // 호출자가 정의한 형식은 생성된 메타데이터를 직접 전달해야 한다.
            var typeInfo = MojangJsonContext.Default.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>;
            if (typeInfo == null)
                throw new NotSupportedException($"Supply a source-generated JsonTypeInfo<{typeof(T).Name}> for custom JSON types.");

            return typeInfo;
        }
    }
}
