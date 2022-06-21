using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MojangAPI.Model
{
    public class NameHistory
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("changedToAt")]
        public long? ChangedToAt { get; set; }

        public DateTime ChangedTime
        {
            get
            {
                if (ChangedToAt == null)
                    return DateTime.MinValue;
                else
                    return DateTimeOffset.FromUnixTimeMilliseconds((long)ChangedToAt).LocalDateTime;
            }
        }
    }
}
