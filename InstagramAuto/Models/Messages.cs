using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ??????
    /// English: Conversation model
    /// </summary>
    public class ConversationDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("participants")]
        public List<string> Participants { get; set; }

        [JsonProperty("last_message")]
        public MessageDto LastMessage { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????
    /// English: Message model
    /// </summary>
    public class MessageDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("conversation_id")]
        public string ConversationId { get; set; }

        [JsonProperty("sender_id")]
        public string SenderId { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("media_url")]
        public string MediaUrl { get; set; }

        [JsonProperty("media_type")]
        public string MediaType { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("sent_at")]
        public DateTimeOffset SentAt { get; set; }

        [JsonProperty("read_at")]
        public DateTimeOffset? ReadAt { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ????? ????
    /// English: Message status model
    /// </summary>
    public class MessageStatusDto
    {
        [JsonProperty("message_id")]
        public string MessageId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("delivered_at")]
        public DateTimeOffset? DeliveredAt { get; set; }

        [JsonProperty("read_at")]
        public DateTimeOffset? ReadAt { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}