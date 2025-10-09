using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ????? ????????
    /// English: Media attachment model
    /// </summary>
    public class MediaAttachment
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("caption")]
        public string Caption { get; set; }
    }

    /// <summary>
    /// Persian: ??? ???? ???? ??????
    /// English: Direct message template model
    /// </summary>
    public class DMTemplate
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("attachments")]
        public List<MediaAttachment> Attachments { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ?????
    /// English: Rule input model
    /// </summary>
    public class RuleIn
    {
        [JsonProperty("account_id")]
        public string Account_id { get; set; }

        [JsonProperty("media_id")]
        public string MediaId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("expression")]
        public string Expression { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ?????
    /// English: Rule output model
    /// </summary>
    public class RuleOut : RuleIn
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset Created_at { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset Updated_at { get; set; }

        [JsonProperty("media_id")]
        public new string MediaId { get; set; }
    }

    /// <summary>
    /// Persian: ??? ???? ?????
    /// English: Complete rule model
    /// </summary>
    public class RuleItem : RuleOut
    {
        [JsonProperty("replies")]
        public List<string> Replies { get; set; } = new List<string>();

        [JsonProperty("attachments")]
        public List<MediaAttachment> Attachments { get; set; } = new List<MediaAttachment>();

        [JsonProperty("send_dm")]
        public bool SendDM { get; set; }

        [JsonProperty("dm")]
        public DMTemplate DM { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????????? ??????
    /// English: Paginated rules model
    /// </summary>
    public class PaginatedRules
    {
        [JsonProperty("items")]
        public List<RuleOut> Items { get; set; }

        [JsonProperty("meta")]
        public PaginationMeta Meta { get; set; }
    }
}