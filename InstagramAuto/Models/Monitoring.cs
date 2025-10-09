using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ????? ?????
    /// English: System health model
    /// </summary>
    public class SystemHealthDto
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("components")]
        public List<ComponentHealth> Components { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("uptime")]
        public TimeSpan Uptime { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ????????
    /// English: Component health model
    /// </summary>
    public class ComponentHealth
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("last_check")]
        public DateTimeOffset LastCheck { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????????? ??????
    /// English: Performance metrics model
    /// </summary>
    public class PerformanceMetricsDto
    {
        [JsonProperty("cpu_usage")]
        public double CpuUsage { get; set; }

        [JsonProperty("memory_usage")]
        public double MemoryUsage { get; set; }

        [JsonProperty("active_connections")]
        public int ActiveConnections { get; set; }

        [JsonProperty("requests_per_second")]
        public double RequestsPerSecond { get; set; }

        [JsonProperty("average_response_time")]
        public double AverageResponseTime { get; set; }

        [JsonProperty("error_rate")]
        public double ErrorRate { get; set; }
    }

    /// <summary>
    /// Persian: ??? ??????? ??????????
    /// English: Monitoring settings model
    /// </summary>
    public class MonitoringConfig
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("metrics_interval_seconds")]
        public int MetricsIntervalSeconds { get; set; }

        [JsonProperty("health_check_interval_seconds")]
        public int HealthCheckIntervalSeconds { get; set; }

        [JsonProperty("alert_thresholds")]
        public Dictionary<string, double> AlertThresholds { get; set; }
    }
}