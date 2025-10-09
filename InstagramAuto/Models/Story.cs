using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ????? ???? ??????
    /// English: Direct message input model
    /// </summary>
    public class DirectMessageIn
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }
    }

    /// <summary>
    /// Persian: ??? ??????
    /// English: Story model
    /// </summary>
    public class StoryDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("user_id")]
        public string User_id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset Created_at { get; set; }

        [JsonProperty("expires_at")]
        public DateTimeOffset Expires_at { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????????? ?????????
    /// English: Paginated stories model
    /// </summary>
    public class PaginatedStoriesDto
    {
        [JsonProperty("items")]
        public List<StoryDto> Items { get; set; }

        [JsonProperty("meta")]
        public PaginationMeta Meta { get; set; }
    }
}