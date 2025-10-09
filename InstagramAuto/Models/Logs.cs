using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: مدل لاگ
    /// English: Log entry model
    /// </summary>
    public class LogEntryDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("account_id")]
        public string Account_id { get; set; }

        [JsonProperty("metadata")]
        public object Metadata { get; set; }
    }

    /// <summary>
    /// Persian: مدل صفحه‌بندی لاگ‌ها
    /// English: Paginated logs model
    /// </summary>
    public class PaginatedLogsDto
    {
        [JsonProperty("items")]
        public LogEntryDto[] Items { get; set; }

        [JsonProperty("meta")]
        public PaginationMeta Meta { get; set; }
    }

    /// <summary>
    /// Persian: مدل صفحه‌بندی شده لاگ‌ها
    /// English: Paginated logs model
    /// </summary>
    public class PaginatedLogs
    {
        [JsonProperty("items")]
        public List<LogItem> Items { get; set; }

        [JsonProperty("meta")]
        public PaginationMeta Meta { get; set; }
    }

    /// <summary>
    /// Persian: مدل آیتم لاگ
    /// English: Log item model
    /// </summary>
    public class LogItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("timestamp")]
        public System.DateTimeOffset Timestamp { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }
}