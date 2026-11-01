using MojangAPI.Model;
using System.Text.Json.Serialization;

namespace MojangAPI.Serialization
{
    [JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(PlayerUUID))]
    [JsonSerializable(typeof(PlayerUUID[]))]
    [JsonSerializable(typeof(PlayerProfile))]
    [JsonSerializable(typeof(NameHistory))]
    [JsonSerializable(typeof(NameHistory[]))]
    [JsonSerializable(typeof(PlayerAttributes))]
    [JsonSerializable(typeof(PlayerCertificates))]
    [JsonSerializable(typeof(Statistics))]
    [JsonSerializable(typeof(MojangAuthResponse))]
    [JsonSerializable(typeof(MicrosoftAuthResponse))]
    [JsonSerializable(typeof(SkinRequest))]
    [JsonSerializable(typeof(StatisticsRequest))]
    [JsonSerializable(typeof(AuthenticationRequest))]
    [JsonSerializable(typeof(TokenRequest))]
    [JsonSerializable(typeof(CredentialsRequest))]
    [JsonSerializable(typeof(string[]))]
    [JsonSerializable(typeof(bool))]
    internal partial class MojangJsonContext : JsonSerializerContext
    {
    }
}
