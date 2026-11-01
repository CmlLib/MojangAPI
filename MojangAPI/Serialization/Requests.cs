using System.Text.Json.Serialization;

namespace MojangAPI.Serialization
{
    internal sealed class SkinRequest
    {
        [JsonPropertyName("variant")]
        public string? Variant { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    internal sealed class StatisticsRequest
    {
        [JsonPropertyName("metricKeys")]
        public string[]? MetricKeys { get; set; }
    }

    internal sealed class CredentialsRequest
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }
    }

    internal sealed class AuthenticationRequest
    {
        [JsonPropertyName("agent")]
        public Agent Agent { get; set; } = new Agent();

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("clientToken")]
        public string? ClientToken { get; set; }
    }

    internal sealed class Agent
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "Minecraft";

        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;
    }

    internal sealed class TokenRequest
    {
        [JsonPropertyName("accessToken")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("clientToken")]
        public string? ClientToken { get; set; }

        [JsonPropertyName("selectedProfile")]
        public SelectedProfile? SelectedProfile { get; set; }
    }

    internal sealed class SelectedProfile
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
