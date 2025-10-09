using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ????? ??????????
    /// English: Instagram media model
    /// </summary>
    public class InstagramMedia
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("media_type")]
        public string MediaType { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("caption")]
        public string Caption { get; set; }

        [JsonProperty("like_count")]
        public int LikeCount { get; set; }

        [JsonProperty("comment_count")]
        public int CommentCount { get; set; }

        [JsonProperty("view_count")]
        public int? ViewCount { get; set; }

        [JsonProperty("taken_at")]
        public DateTimeOffset TakenAt { get; set; }

        [JsonProperty("product_type")]
        public string ProductType { get; set; }

        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("image_versions")]
        public List<MediaVersion> ImageVersions { get; set; }

        [JsonProperty("video_versions")]
        public List<MediaVersion> VideoVersions { get; set; }

        [JsonProperty("carousel_media")]
        public List<CarouselMedia> CarouselMedia { get; set; }
    }

    /// <summary>
    /// Persian: ??? ???? ?????
    /// English: Media version model
    /// </summary>
    public class MediaVersion
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ???????
    /// English: Carousel media model
    /// </summary>
    public class CarouselMedia
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("media_type")]
        public string MediaType { get; set; }

        [JsonProperty("image_versions")]
        public List<MediaVersion> ImageVersions { get; set; }

        [JsonProperty("video_versions")]
        public List<MediaVersion> VideoVersions { get; set; }

        [JsonProperty("original_width")]
        public int OriginalWidth { get; set; }

        [JsonProperty("original_height")]
        public int OriginalHeight { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ?????
    /// English: Media filter model
    /// </summary>
    public class MediaFilter
    {
        [JsonProperty("types")]
        public List<string> Types { get; set; }

        [JsonProperty("min_like_count")]
        public int? MinLikeCount { get; set; }

        [JsonProperty("min_comment_count")]
        public int? MinCommentCount { get; set; }

        [JsonProperty("date_from")]
        public DateTimeOffset? DateFrom { get; set; }

        [JsonProperty("date_to")]
        public DateTimeOffset? DateTo { get; set; }

        [JsonProperty("has_caption")]
        public bool? HasCaption { get; set; }

        [JsonProperty("caption_contains")]
        public List<string> CaptionContains { get; set; }
    }
}