using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace InstagramAuto.Client
{
    public class CommentItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset Created_at { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties { get; set; }
    }

    public class PaginatedComments
    {
        [JsonProperty("items")]
        public List<CommentItem> Items { get; set; }

        [JsonProperty("meta")]
        public PaginationMeta Meta { get; set; }
    }
}
