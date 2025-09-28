using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian:
    ///   یک ورودی لاگ شامل زمان، سطح (Error, Warning, Info) و پیام.
    /// English:
    ///   A log entry containing timestamp, level, and message.
    /// </summary>
    public class LogEntryDto
    {
        /// <summary>Persian: شناسه‌ی لاگ | English: Log identifier</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>Persian: زمان رخداد | English: Occurrence timestamp</summary>
        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>Persian: سطح لاگ | English: Log level (Error, Warning, Info)</summary>
        [JsonProperty("level")]
        public string Level { get; set; }

        /// <summary>Persian: متن پیام | English: Log message</summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Persian:
    ///   پاسخ صفحه‌بندی برای لاگ‌ها شامل آیتم‌ها و متادیتا.
    /// English:
    ///   Pagination response for logs, containing items and metadata.
    /// </summary>
    public class PaginatedLogsDto
    {
        /// <summary>Persian: لیست لاگ‌ها | English: Collection of log entries</summary>
        [JsonProperty("items")]
        public ICollection<LogEntryDto> Items { get; set; }

        /// <summary>Persian: متادیتای صفحه‌بندی (cursor و تعداد) | English: Pagination metadata (cursor and counts)</summary>
        [JsonProperty("meta")]
        public PaginationMeta Meta { get; set; }
    }
}
