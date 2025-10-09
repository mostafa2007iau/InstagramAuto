using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace InstagramAuto.Client.Models
{
    public class SettingsDto
    {
        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("enable_delay")]
        public bool EnableDelay { get; set; }

        [JsonProperty("comment_delay_sec")]
        public int CommentDelaySec { get; set; }

        [JsonProperty("like_delay_sec")]
        public int LikeDelaySec { get; set; }

        [JsonProperty("dm_delay_sec")]
        public int DmDelaySec { get; set; }

        [JsonProperty("hourly_limit")]
        public int HourlyLimit { get; set; }

        [JsonProperty("daily_limit")]
        public int DailyLimit { get; set; }

        [JsonProperty("random_jitter_enabled")]
        public bool RandomJitterEnabled { get; set; }

        [JsonProperty("jitter_min_sec")]
        public int JitterMinSec { get; set; }

        [JsonProperty("jitter_max_sec")]
        public int JitterMaxSec { get; set; }
    }

    public class SettingsIn
    {
        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("enable_delay")]
        public bool EnableDelay { get; set; }

        [JsonProperty("comment_delay_sec")]
        public int CommentDelaySec { get; set; }

        [JsonProperty("like_delay_sec")]
        public int LikeDelaySec { get; set; }

        [JsonProperty("dm_delay_sec")]
        public int DmDelaySec { get; set; }

        [JsonProperty("hourly_limit")]
        public int HourlyLimit { get; set; }

        [JsonProperty("daily_limit")]
        public int DailyLimit { get; set; }

        [JsonProperty("random_jitter_enabled")]
        public bool RandomJitterEnabled { get; set; }

        [JsonProperty("jitter_min_sec")]
        public int JitterMinSec { get; set; }

        [JsonProperty("jitter_max_sec")]
        public int JitterMaxSec { get; set; }
    }

    public class ProxyTestResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public class ProxyToggleResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
