using System;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ????
    /// English: Like model
    /// </summary>
    public class LikeResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("likes_count")]
        public int LikesCount { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Persian: ??? ??? ????
    /// English: New comment model
    /// </summary>
    public class CommentCreateInput
    {
        [JsonProperty("media_id")]
        public string MediaId { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("reply_to_comment_id")]
        public string ReplyToCommentId { get; set; }
    }

    /// <summary>
    /// Persian: ???? ?????
    /// English: Comment response model
    /// </summary>
    public class CommentResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("comment_id")]
        public string CommentId { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????????
    /// English: Engagement model
    /// </summary>
    public class EngagementMetrics
    {
        [JsonProperty("likes_count")]
        public int LikesCount { get; set; }

        [JsonProperty("comments_count")]
        public int CommentsCount { get; set; }

        [JsonProperty("shares_count")]
        public int SharesCount { get; set; }

        [JsonProperty("saves_count")]
        public int SavesCount { get; set; }

        [JsonProperty("engagement_rate")]
        public double EngagementRate { get; set; }

        [JsonProperty("reach")]
        public int Reach { get; set; }

        [JsonProperty("impressions")]
        public int Impressions { get; set; }
    }
}