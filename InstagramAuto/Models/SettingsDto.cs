using System;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian:
    ///   مدل تنظیمات کاربر در پاسخ GET/PUT.
    /// English:
    ///   DTO for user settings returned by GET/PUT endpoints.
    /// </summary>
    public class SettingsDto
    {
        [JsonProperty("account_id", Required = Required.Always)]
        public string AccountId { get; set; }

        [JsonProperty("enable_delay", Required = Required.DisallowNull)]
        public bool EnableDelay { get; set; }

        [JsonProperty("comment_delay_sec", Required = Required.DisallowNull)]
        public int CommentDelaySec { get; set; }

        [JsonProperty("like_delay_sec", Required = Required.DisallowNull)]
        public int LikeDelaySec { get; set; }

        [JsonProperty("dm_delay_sec", Required = Required.DisallowNull)]
        public int DmDelaySec { get; set; }

        /// <summary>Persian: حداکثر تعداد اکشن در هر ساعت | English: Maximum actions per hour</summary>
        [JsonProperty("hourly_limit", Required = Required.DisallowNull)]
        public int HourlyLimit { get; set; }

        /// <summary>Persian: حداکثر تعداد اکشن در هر روز | English: Maximum actions per day</summary>
        [JsonProperty("daily_limit", Required = Required.DisallowNull)]
        public int DailyLimit { get; set; }

        /// <summary>Persian: فعال کردن تأخیر تصادفی | English: Enable random jitter delay</summary>
        [JsonProperty("random_jitter_enabled", Required = Required.DisallowNull)]
        public bool RandomJitterEnabled { get; set; }

        /// <summary>Persian: حداقل بازه تأخیر تصادفی (ثانیه) | English: Minimum random jitter in seconds</summary>
        [JsonProperty("jitter_min_sec", Required = Required.DisallowNull)]
        public int JitterMinSec { get; set; }

        /// <summary>Persian: حداکثر بازه تأخیر تصادفی (ثانیه) | English: Maximum random jitter in seconds</summary>
        [JsonProperty("jitter_max_sec", Required = Required.DisallowNull)]
        public int JitterMaxSec { get; set; }

        [JsonProperty("created_at", Required = Required.DisallowNull)]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at", Required = Required.DisallowNull)]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    /// <summary>
    /// Persian:
    ///   مدل ورودی برای بروزرسانی تنظیمات (PUT).
    /// English:
    ///   Input DTO for updating user settings (PUT).
    /// </summary>
    public class SettingsIn
    {
        [JsonProperty("account_id", Required = Required.Always)]
        public string AccountId { get; set; }

        [JsonProperty("enable_delay", Required = Required.DisallowNull)]
        public bool EnableDelay { get; set; }

        [JsonProperty("comment_delay_sec", Required = Required.DisallowNull)]
        public int CommentDelaySec { get; set; }

        [JsonProperty("like_delay_sec", Required = Required.DisallowNull)]
        public int LikeDelaySec { get; set; }

        [JsonProperty("dm_delay_sec", Required = Required.DisallowNull)]
        public int DmDelaySec { get; set; }

        [JsonProperty("hourly_limit", Required = Required.DisallowNull)]
        public int HourlyLimit { get; set; }

        [JsonProperty("daily_limit", Required = Required.DisallowNull)]
        public int DailyLimit { get; set; }

        [JsonProperty("random_jitter_enabled", Required = Required.DisallowNull)]
        public bool RandomJitterEnabled { get; set; }

        [JsonProperty("jitter_min_sec", Required = Required.DisallowNull)]
        public int JitterMinSec { get; set; }

        [JsonProperty("jitter_max_sec", Required = Required.DisallowNull)]
        public int JitterMaxSec { get; set; }
    }
}
