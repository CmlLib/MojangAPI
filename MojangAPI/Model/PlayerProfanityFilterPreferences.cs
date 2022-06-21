using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MojangAPI.Model
{
    public class PlayerProfanityFilterPreferences
    {
        [JsonProperty("profanityFilterOn")]
        public bool ProfanityFilterOn { get; set; }
    }
}
