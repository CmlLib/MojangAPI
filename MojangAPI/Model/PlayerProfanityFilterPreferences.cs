using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MojangAPI.Model
{
    public class PlayerProfanityFilterPreferences
    {
        [JsonPropertyName("profanityFilterOn")]
        public bool ProfanityFilterOn { get; set; }
    }
}
