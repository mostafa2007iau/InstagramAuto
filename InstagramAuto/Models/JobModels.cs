using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian:
    ///   DTO برای نمایش خلاصه‌ی یک وظیفه در فهرست.
    /// English:
    ///   DTO representing a summary of a job in the list.
    /// </summary>
    public class JobSummaryDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("target_info")]
        public string TargetInfo { get; set; }
    }

    /// <summary>
    /// Persian:
    ///   DTO برای نمایش جزئیات یک وظیفه.
    /// English:
    ///   DTO representing detailed information of a job.
    /// </summary>
    public class JobDetailDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("target_info")]
        public string TargetInfo { get; set; }

        [JsonProperty("attempts")]
        public int Attempts { get; set; }

        [JsonProperty("payload_raw")]
        public string PayloadRaw { get; set; }
    }

    /// <summary>
    /// Persian:
    ///   ساختار پاسخ صفحه‌بندی برای jobها.
    /// English:
    ///   Pagination response wrapper for jobs.
    /// </summary>
    public class PaginatedJobsDto
    {
        [JsonProperty("items")]
        public ICollection<JobSummaryDto> Items { get; set; }

        [JsonProperty("meta")]
        public JobPaginationMeta Meta { get; set; }
    }

    /// <summary>
    /// Persian:
    ///   متادیتای صفحه‌بندی (cursor، تعداد).
    /// English:
    ///   Pagination metadata (next cursor, counts).
    /// </summary>
    public class JobPaginationMeta
    {
        [JsonProperty("next_cursor")]
        public string NextCursor { get; set; }

        [JsonProperty("count_queued")]
        public int CountQueued { get; set; }

        [JsonProperty("count_sent")]
        public int CountSent { get; set; }
    }

    /// <summary>
    /// Persian:
    ///   نتیجهٔ یک عملیات مدیریتی (Retry/Cancel/DLQ).
    /// English:
    ///   Result of an administrative action (Retry/Cancel/DLQ).
    /// </summary>
    public class ApiActionResult
    {
        [JsonProperty("ok")]
        public bool Ok { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
