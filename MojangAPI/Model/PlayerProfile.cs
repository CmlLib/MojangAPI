using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MojangAPI.Model
{
    public class PlayerProfile
    {
        [JsonPropertyName("id")]
        public string? UUID { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("legacy")]
        public bool IsLegacy { get; set; }

        public Skin? Skin { get; set; }
    }
}
