﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MojangAPI.Model
{
    public class PlayerUUID
    {
        [JsonPropertyName("id")]
        public string? UUID { get; set; }

        [JsonPropertyName("name")]
        public string? CurrentUsername { get; set; }

        [JsonPropertyName("legacy")]
        public bool IsLegacy { get; set; }

        [JsonPropertyName("demo")]
        public bool IsDemo { get; set; }
    }
}
