using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ??????? ??????
    /// English: Application configuration model
    /// </summary>
    public class AppConfig
    {
        [JsonProperty("api_base_url")]
        public string ApiBaseUrl { get; set; }

        [JsonProperty("timeout_seconds")]
        public int TimeoutSeconds { get; set; }

        [JsonProperty("max_retry_attempts")]
        public int MaxRetryAttempts { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("theme")]
        public string Theme { get; set; }

        [JsonProperty("notifications_enabled")]
        public bool NotificationsEnabled { get; set; }
    }

    /// <summary>
    /// Persian: ??? ??????? ?????
    /// English: User preferences model
    /// </summary>
    public class UserPreferences
    {
        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("notification_settings")]
        public NotificationPreferences NotificationSettings { get; set; }

        [JsonProperty("display_settings")]
        public DisplayPreferences DisplaySettings { get; set; }

        [JsonProperty("automation_settings")]
        public AutomationPreferences AutomationSettings { get; set; }
    }

    /// <summary>
    /// Persian: ??? ??????? ????????
    /// English: Notification preferences model
    /// </summary>
    public class NotificationPreferences
    {
        [JsonProperty("email_notifications")]
        public bool EmailNotifications { get; set; }

        [JsonProperty("push_notifications")]
        public bool PushNotifications { get; set; }

        [JsonProperty("notification_types")]
        public List<string> NotificationTypes { get; set; }

        [JsonProperty("quiet_hours")]
        public Dictionary<string, TimeRange> QuietHours { get; set; }
    }

    /// <summary>
    /// Persian: ??? ??????? ??????
    /// English: Display preferences model
    /// </summary>
    public class DisplayPreferences
    {
        [JsonProperty("theme")]
        public string Theme { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("date_format")]
        public string DateFormat { get; set; }

        [JsonProperty("compact_view")]
        public bool CompactView { get; set; }
    }

    /// <summary>
    /// Persian: ??? ??????? ?????????
    /// English: Automation preferences model
    /// </summary>
    public class AutomationPreferences
    {
        [JsonProperty("auto_reply_enabled")]
        public bool AutoReplyEnabled { get; set; }

        [JsonProperty("working_hours")]
        public Dictionary<string, TimeRange> WorkingHours { get; set; }

        [JsonProperty("blackout_days")]
        public List<DayOfWeek> BlackoutDays { get; set; }

        [JsonProperty("max_daily_actions")]
        public int MaxDailyActions { get; set; }
    }

    /// <summary>
    /// Persian: ??? ???? ?????
    /// English: Time range model
    /// </summary>
    public class TimeRange
    {
        [JsonProperty("start")]
        public TimeSpan Start { get; set; }

        [JsonProperty("end")]
        public TimeSpan End { get; set; }
    }
}