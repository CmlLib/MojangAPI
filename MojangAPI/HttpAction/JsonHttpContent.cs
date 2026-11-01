using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
#if NET10_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace HttpAction
{
    public class JsonHttpContent : StringContent
    {
        private const string DefaultMediaType = "application/json";

#if NET10_0_OR_GREATER
        [RequiresUnreferencedCode("Use Create with source-generated JSON metadata.")]
        [RequiresDynamicCode("Use Create with source-generated JSON metadata.")]
#endif
        public JsonHttpContent(object obj)
            : this(obj, null)
        {

        }

#if NET10_0_OR_GREATER
        [RequiresUnreferencedCode("Use Create with source-generated JSON metadata.")]
        [RequiresDynamicCode("Use Create with source-generated JSON metadata.")]
#endif
        public JsonHttpContent(object obj, Encoding? encoding)
            : this(obj, encoding, DefaultMediaType)
        {

        }

#if NET10_0_OR_GREATER
        [RequiresUnreferencedCode("Use Create with source-generated JSON metadata.")]
        [RequiresDynamicCode("Use Create with source-generated JSON metadata.")]
#endif
        public JsonHttpContent(object obj, Encoding? encoding, string mediaType)
            : base(serialize(obj), encoding, mediaType)
        {
            
        }

        private JsonHttpContent(string json, Encoding? encoding, string mediaType)
            : base(json, encoding, mediaType)
        {
        }

        public static JsonHttpContent Create<T>(T value, JsonTypeInfo<T> typeInfo)
        {
            return new JsonHttpContent(JsonSerializer.Serialize(value, typeInfo), Encoding.UTF8, DefaultMediaType);
        }

#if NET10_0_OR_GREATER
        [RequiresUnreferencedCode("Use Create with source-generated JSON metadata.")]
        [RequiresDynamicCode("Use Create with source-generated JSON metadata.")]
#endif
        public static string serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
    }
}
