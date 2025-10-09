using System;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ????? ??????
    /// English: Stream connection model
    /// </summary>
    public class StreamConnection
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("connected_at")]
        public DateTimeOffset ConnectedAt { get; set; }

        [JsonProperty("last_ping")]
        public DateTimeOffset LastPing { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    /// <summary>
    /// Persian: ??? ?????? ??????
    /// English: Stream event model
    /// </summary>
    public class StreamEvent
    {
        [JsonProperty("connection_id")]
        public string ConnectionId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }

    /// <summary>
    /// Persian: ??? ?????? ??????
    /// English: Stream subscription model
    /// </summary>
    public class StreamSubscription
    {
        [JsonProperty("connection_id")]
        public string ConnectionId { get; set; }

        [JsonProperty("event_types")]
        public string[] EventTypes { get; set; }

        [JsonProperty("filters")]
        public object Filters { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }

    /// <summary>
    /// Persian: ??? ??????? ??????
    /// English: Stream settings model
    /// </summary>
    public class StreamSettings
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("ping_interval_seconds")]
        public int PingIntervalSeconds { get; set; }

        [JsonProperty("connection_timeout_seconds")]
        public int ConnectionTimeoutSeconds { get; set; }

        [JsonProperty("max_connections_per_account")]
        public int MaxConnectionsPerAccount { get; set; }

        [JsonProperty("buffer_size")]
        public int BufferSize { get; set; }
    }
}