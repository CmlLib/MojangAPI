using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MojangAPI.Model
{
    public class NameHistory : MojangAPIResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("changedToAt")]
        public long ChangedToAt { get; set; }

        [JsonProperty("changedToAt")]
        public DateTime ChangedTime { get => DateTimeOffset.FromUnixTimeMilliseconds(ChangedToAt).UtcDateTime; }
    }
}
