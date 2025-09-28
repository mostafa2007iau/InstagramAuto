using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian:
    ///   نمایش یک استوری اینستاگرام.
    /// English:
    ///   DTO representing a single Instagram story.
    /// </summary>
    public class StoryDto
    {
        /// <summary>Persian: شناسه‌ی استوری | English: Story identifier</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>Persian: نام کاربری صاحب استوری | English: Story owner username</summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>Persian: آدرس تصویر/ویدیو | English: URL of the story media</summary>
        [JsonProperty("media_url")]
        public string MediaUrl { get; set; }

        /// <summary>Persian: زمان اشتراک‌گذاری استوری | English: Timestamp when story was shared</summary>
        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }

    /// <summary>
    /// Persian:
    ///   پاسخ صفحه‌بندی استوری‌ها.
    /// English:
    ///   Pagination response for stories.
    /// </summary>
    public class PaginatedStoriesDto
    {
        /// <summary>Persian: لیست استوری‌ها | English: Collection of stories</summary>
        [JsonProperty("items")]
        public ICollection<StoryDto> Items { get; set; }

        /// <summary>Persian: متادیتای صفحه‌بندی | English: Pagination metadata</summary>
        [JsonProperty("meta")]
        public PaginationMeta Meta { get; set; }
    }

    /// <summary>
    /// Persian:
    ///   ورودی برای ارسال پاسخ به استوری (دایرکت).
    /// English:
    ///   Input DTO for replying to a story via DM.
    /// </summary>
    public class DirectMessageIn
    {
        /// <summary>Persian: متن پیام | English: Message text</summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>Persian: لینک اختیاری | English: Optional link URL</summary>
        [JsonProperty("link_url", NullValueHandling = NullValueHandling.Ignore)]
        public string LinkUrl { get; set; }

        /// <summary>Persian: آدرس تصویر اختیاری | English: Optional image URL</summary>
        [JsonProperty("image_url", NullValueHandling = NullValueHandling.Ignore)]
        public string ImageUrl { get; set; }
    }
}
