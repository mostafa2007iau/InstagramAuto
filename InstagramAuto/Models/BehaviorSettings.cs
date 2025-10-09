using System;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ??????? ??????? ??? ???????
    /// English: Rate limit settings model
    /// </summary>
    public class RateLimitSettings
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("requests_per_hour")]
        public int RequestsPerHour { get; set; }

        [JsonProperty("requests_per_day")]
        public int RequestsPerDay { get; set; }

        [JsonProperty("min_delay_ms")]
        public int MinDelayMs { get; set; }
    }

    /// <summary>
    /// Persian: ??????? ????? ??????????
    /// English: Human behavior settings
    /// </summary>
    public class HumanBehaviorSettings
    {
        [JsonProperty("random_delays")]
        public bool RandomDelays { get; set; }

        [JsonProperty("typing_simulation")]
        public bool TypingSimulation { get; set; }

        [JsonProperty("view_stories")]
        public bool ViewStories { get; set; }

        [JsonProperty("like_posts")]
        public bool LikePosts { get; set; }

        [JsonProperty("min_typing_delay_ms")]
        public int MinTypingDelayMs { get; set; }

        [JsonProperty("max_typing_delay_ms")]
        public int MaxTypingDelayMs { get; set; }
    }
}