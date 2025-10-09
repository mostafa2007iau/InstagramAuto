using System;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ???? API
    /// English: API error model
    /// </summary>
    public class ApiError
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("details")]
        public string Details { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("request_id")]
        public string RequestId { get; set; }
    }

    /// <summary>
    /// Persian: ??? ?????
    /// English: Notification model
    /// </summary>
    public class NotificationDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("severity")]
        public string Severity { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("read")]
        public bool Read { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }

    /// <summary>
    /// Persian: ??? ???? ???? ???
    /// English: Parsed error model
    /// </summary>
    public class ParsedError
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("details")]
        public string Details { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }

        [JsonProperty("stack_trace")]
        public string StackTrace { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ?????
    /// English: System status model
    /// </summary>
    public class SystemStatus
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("last_check")]
        public DateTimeOffset LastCheck { get; set; }

        [JsonProperty("warnings")]
        public string[] Warnings { get; set; }

        [JsonProperty("errors")]
        public string[] Errors { get; set; }
    }
}