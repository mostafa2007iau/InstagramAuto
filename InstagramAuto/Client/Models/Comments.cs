using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ????????? ??? ????????
    /// English: Paginated comments model
    /// </summary>
    public class PaginatedComments
    {
        [JsonProperty("items")]
        public List<CommentItem> Items { get; set; }

        [JsonProperty("meta")]
        public PaginationMeta Meta { get; set; }
    }

    /// <summary>
    /// Persian: ??? ?????
    /// English: Comment model
    /// </summary>
    public class CommentItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }

    /// <summary>
    /// Persian: ????? ???? API
    /// English: API action result
    /// </summary>
    public class ApiActionResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}