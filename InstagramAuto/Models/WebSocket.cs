using System;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ???? ????
    /// English: Challenge envelope model
    /// </summary>
    public class ChallengeEnvelope
    {
        [JsonProperty("state")]
        public ChallengeState State { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }

    /// <summary>
    /// Persian: ??? ?????? ????
    /// English: Challenge event model
    /// </summary>
    public class ChallengeEvent
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }

    /// <summary>
    /// Persian: ??? ?????? ???????
    /// English: WebSocket event model
    /// </summary>
    public class WebSocketEvent
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("payload")]
        public object Payload { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ????? ???????
    /// English: WebSocket connection state model
    /// </summary>
    public class WebSocketConnectionState
    {
        [JsonProperty("connected")]
        public bool Connected { get; set; }

        [JsonProperty("last_ping")]
        public DateTimeOffset? LastPing { get; set; }

        [JsonProperty("connection_id")]
        public string ConnectionId { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}