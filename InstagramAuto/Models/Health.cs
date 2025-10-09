using System;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ????? ????? ?????
    /// English: Service health status
    /// </summary>
    public class HealthResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("uptime")]
        public TimeSpan Uptime { get; set; }

        [JsonProperty("components")]
        public HealthComponentStatus[] Components { get; set; }
    }

    /// <summary>
    /// Persian: ????? ????? ????????
    /// English: Component health status
    /// </summary>
    public class HealthComponentStatus
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("last_check")]
        public DateTimeOffset Last_check { get; set; }
    }

    /// <summary>
    /// Persian: ????????? ???????
    /// English: Performance metrics
    /// </summary>
    public class PerformanceMetrics
    {
        [JsonProperty("total_requests")]
        public long Total_requests { get; set; }

        [JsonProperty("active_sessions")]
        public int Active_sessions { get; set; }

        [JsonProperty("queued_jobs")]
        public int Queued_jobs { get; set; }

        [JsonProperty("failed_jobs")]
        public int Failed_jobs { get; set; }

        [JsonProperty("average_response_time_ms")]
        public double Average_response_time_ms { get; set; }
    }

    public class HealthStatus
    {
        [JsonProperty("instagram")]
        public HealthComponentStatus Instagram { get; set; }
        [JsonProperty("queue")]
        public HealthComponentStatus Queue { get; set; }
        [JsonProperty("database")]
        public HealthComponentStatus Database { get; set; }
        [JsonProperty("cache")]
        public HealthComponentStatus Cache { get; set; }
    }
}