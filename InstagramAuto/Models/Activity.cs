using System;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: مدل فعالیت
    /// English: Activity model
    /// </summary>
    public class ActivityItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset Created_at { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset Updated_at { get; set; }

        [JsonProperty("account_id")]
        public string Account_id { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }

    /// <summary>
    /// Persian: مدل آمار پاسخ
    /// English: Reply statistics model
    /// </summary>
    public class ReplyStatItem
    {
        [JsonProperty("media_id")]
        public string Media_id { get; set; }

        [JsonProperty("total_replies")]
        public int Total_replies { get; set; }

        [JsonProperty("successful_replies")]
        public int Successful_replies { get; set; }

        [JsonProperty("failed_replies")]
        public int Failed_replies { get; set; }

        [JsonProperty("last_reply_at")]
        public DateTimeOffset? Last_reply_at { get; set; }
    }
}