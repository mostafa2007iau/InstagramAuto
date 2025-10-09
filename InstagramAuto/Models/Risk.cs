using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??????? ????
    /// English: Risk management settings
    /// </summary>
    public class RiskSettings
    {
        [JsonProperty("cooldown_minutes")]
        public int CooldownMinutes { get; set; }

        [JsonProperty("auto_pause")]
        public bool AutoPause { get; set; }

        [JsonProperty("limitations_enabled")]
        public bool LimitationsEnabled { get; set; }

        [JsonProperty("random_delay_percentage")]
        public double RandomDelayPercentage { get; set; }

        [JsonProperty("max_actions_per_day")]
        public Dictionary<ActionType, int> MaxActionsPerDay { get; set; } = new();

        [JsonProperty("max_actions_per_hour")]
        public Dictionary<ActionType, int> MaxActionsPerHour { get; set; } = new();

        [JsonProperty("min_delay_between_actions")]
        public Dictionary<ActionType, int> MinDelayBetweenActions { get; set; } = new();
        // ✅ پراپرتی ترکیبی
        [JsonIgnore] // چون می‌سازی از سه پراپرتی بالا، لازم نیست در JSON ذخیره بشه
        public Dictionary<ActionType, ActionLimits> Limits
        {
            get
            {
                var dict = new Dictionary<ActionType, ActionLimits>();
                foreach (var type in Enum.GetValues<ActionType>())
                {
                    dict[type] = new ActionLimits
                    {
                        DailyLimit = MaxActionsPerDay.TryGetValue(type, out var d) ? d : 0,
                        HourlyLimit = MaxActionsPerHour.TryGetValue(type, out var h) ? h : 0,
                        MinIntervalSeconds = MinDelayBetweenActions.TryGetValue(type, out var m) ? m : 0
                    };
                }
                return dict;
            }
            set
            {
                if (value == null) return;
                foreach (var kv in value)
                {
                    MaxActionsPerDay[kv.Key] = kv.Value.DailyLimit;
                    MaxActionsPerHour[kv.Key] = kv.Value.HourlyLimit;
                    MinDelayBetweenActions[kv.Key] = kv.Value.MinIntervalSeconds;
                }
            }
        }
    }

    /// <summary>
    /// Persian: ??????????? ????
    /// English: Action limits configuration
    /// </summary>
    public class ActionLimits
    {
        [JsonProperty("hourly_limit")]
        public int HourlyLimit { get; set; }

        [JsonProperty("daily_limit")]
        public int DailyLimit { get; set; }

        [JsonProperty("min_interval_seconds")]
        public int MinIntervalSeconds { get; set; }
    }

    /// <summary>
    /// Persian: ????? ???????
    /// English: Types of actions that can be rate-limited
    /// </summary>
    public enum ActionType
    {
        CommentReply,
        DirectMessage,
        Like,
        Follow,
        Story,
        Post
    }
}