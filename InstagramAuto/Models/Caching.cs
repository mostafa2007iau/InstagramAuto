using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ?????? ??
    /// English: Cache management model
    /// </summary>
    public class CacheEntry
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("expires_at")]
        public DateTimeOffset? ExpiresAt { get; set; }

        [JsonProperty("last_accessed")]
        public DateTimeOffset LastAccessed { get; set; }

        [JsonProperty("access_count")]
        public int AccessCount { get; set; }
    }

    /// <summary>
    /// Persian: ??? ??????? ??
    /// English: Cache settings model
    /// </summary>
    public class CacheSettings
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("default_ttl_minutes")]
        public int DefaultTtlMinutes { get; set; }

        [JsonProperty("max_entries")]
        public int MaxEntries { get; set; }

        [JsonProperty("cleanup_interval_minutes")]
        public int CleanupIntervalMinutes { get; set; }

        [JsonProperty("type_settings")]
        public Dictionary<string, TypeCacheSettings> TypeSettings { get; set; }
    }

    /// <summary>
    /// Persian: ??? ??????? ?? ???? ???
    /// English: Type cache settings model
    /// </summary>
    public class TypeCacheSettings
    {
        [JsonProperty("ttl_minutes")]
        public int TtlMinutes { get; set; }

        [JsonProperty("max_entries")]
        public int MaxEntries { get; set; }

        [JsonProperty("refresh_enabled")]
        public bool RefreshEnabled { get; set; }

        [JsonProperty("refresh_interval_minutes")]
        public int RefreshIntervalMinutes { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ??
    /// English: Cache status model
    /// </summary>
    public class CacheStatus
    {
        [JsonProperty("total_entries")]
        public int TotalEntries { get; set; }

        [JsonProperty("memory_usage_bytes")]
        public long MemoryUsageBytes { get; set; }

        [JsonProperty("hit_rate")]
        public double HitRate { get; set; }

        [JsonProperty("miss_rate")]
        public double MissRate { get; set; }

        [JsonProperty("entries_by_type")]
        public Dictionary<string, int> EntriesByType { get; set; }

        [JsonProperty("last_cleanup")]
        public DateTimeOffset LastCleanup { get; set; }
    }
}