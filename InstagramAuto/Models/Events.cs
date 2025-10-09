using System;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ??????
    /// English: Event model
    /// </summary>
    public class EventDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }

    /// <summary>
    /// Persian: ??? ???????? ?????
    /// English: Webhook configuration model
    /// </summary>
    public class WebhookConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("events")]
        public string[] Events { get; set; }

        [JsonProperty("secret")]
        public string Secret { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ?????
    /// English: Webhook delivery model
    /// </summary>
    public class WebhookDelivery
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("webhook_id")]
        public string WebhookId { get; set; }

        [JsonProperty("event_id")]
        public string EventId { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("status_code")]
        public int StatusCode { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("response_body")]
        public string ResponseBody { get; set; }

        [JsonProperty("retry_count")]
        public int RetryCount { get; set; }
    }

    /// <summary>
    /// Persian: ??? ???? ?????
    /// English: Webhook response model
    /// </summary>
    public class WebhookResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("delivery_id")]
        public string DeliveryId { get; set; }
    }
}