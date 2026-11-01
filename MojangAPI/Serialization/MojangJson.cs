using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace MojangAPI.Serialization
{
    internal static class MojangJson
    {
        internal static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        internal static T? Deserialize<T>(string json, JsonTypeInfo<T>? typeInfo = null)
        {
#if NET10_0_OR_GREATER
            return JsonSerializer.Deserialize(json, typeInfo ?? GetTypeInfo<T>());
#else
            if (typeInfo != null)
                return JsonSerializer.Deserialize(json, typeInfo);

            return JsonSerializer.Deserialize<T>(json, Options);
#endif
        }

        internal static string Serialize<T>(T value, JsonTypeInfo<T>? typeInfo = null)
        {
#if NET10_0_OR_GREATER
            return JsonSerializer.Serialize(value, typeInfo ?? GetTypeInfo<T>());
#else
            if (typeInfo != null)
                return JsonSerializer.Serialize(value, typeInfo);

            return JsonSerializer.Serialize(value);
#endif
        }

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
