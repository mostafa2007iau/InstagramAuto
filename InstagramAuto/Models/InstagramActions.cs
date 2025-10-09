using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ???? ??????????
    /// English: Instagram action model
    /// </summary>
    public class InstagramAction
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("target_id")]
        public string TargetId { get; set; }

        [JsonProperty("target_type")]
        public string TargetType { get; set; }

        [JsonProperty("params")]
        public Dictionary<string, object> Params { get; set; }

        [JsonProperty("performed_at")]
        public DateTimeOffset PerformedAt { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    /// <summary>
    /// Persian: ??? ??????? ????
    /// English: Action limit model
    /// </summary>
    public class ActionLimit
    {
        [JsonProperty("action_type")]
        public string ActionType { get; set; }

        [JsonProperty("hourly_limit")]
        public int HourlyLimit { get; set; }

        [JsonProperty("daily_limit")]
        public int DailyLimit { get; set; }

        [JsonProperty("current_hour_count")]
        public int CurrentHourCount { get; set; }

        [JsonProperty("current_day_count")]
        public int CurrentDayCount { get; set; }

        [JsonProperty("reset_hour")]
        public DateTimeOffset ResetHour { get; set; }

        [JsonProperty("reset_day")]
        public DateTimeOffset ResetDay { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ??????????
    /// English: Rate limits status model
    /// </summary>
    public class RateLimitsStatus
    {
        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("action_limits")]
        public Dictionary<string, ActionLimit> ActionLimits { get; set; }

        [JsonProperty("is_blocked")]
        public bool IsBlocked { get; set; }

        [JsonProperty("block_expires_at")]
        public DateTimeOffset? BlockExpiresAt { get; set; }

        [JsonProperty("warnings")]
        public List<string> Warnings { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ??????
    /// English: Activity report model
    /// </summary>
    public class ActivityReport
    {
        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("period_start")]
        public DateTimeOffset PeriodStart { get; set; }

        [JsonProperty("period_end")]
        public DateTimeOffset PeriodEnd { get; set; }

        [JsonProperty("actions_by_type")]
        public Dictionary<string, int> ActionsByType { get; set; }

        [JsonProperty("success_rate")]
        public double SuccessRate { get; set; }

        [JsonProperty("error_counts")]
        public Dictionary<string, int> ErrorCounts { get; set; }
    }
}