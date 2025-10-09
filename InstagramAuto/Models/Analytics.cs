using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ???? ???
    /// English: General statistics model
    /// </summary>
    public class StatisticsDto
    {
        [JsonProperty("total_messages_sent")]
        public long TotalMessagesSent { get; set; }

        [JsonProperty("total_comments_posted")]
        public long TotalCommentsPosted { get; set; }

        [JsonProperty("total_stories_watched")]
        public long TotalStoriesWatched { get; set; }

        [JsonProperty("total_likes_given")]
        public long TotalLikesGiven { get; set; }

        [JsonProperty("success_rate")]
        public double SuccessRate { get; set; }

        [JsonProperty("average_response_time_ms")]
        public double AverageResponseTimeMs { get; set; }
    }

    /// <summary>
    /// Persian: ??? ???? ?????
    /// English: Time-based analytics model
    /// </summary>
    public class TimeAnalyticsDto
    {
        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("messages_count")]
        public int MessagesCount { get; set; }

        [JsonProperty("comments_count")]
        public int CommentsCount { get; set; }

        [JsonProperty("stories_count")]
        public int StoriesCount { get; set; }

        [JsonProperty("likes_count")]
        public int LikesCount { get; set; }

        [JsonProperty("success_rate")]
        public double SuccessRate { get; set; }
    }

    /// <summary>
    /// Persian: ??? ???? ??????
    /// English: Daily statistics model
    /// </summary>
    public class DailyStatsDto
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("total_actions")]
        public int TotalActions { get; set; }

        [JsonProperty("successful_actions")]
        public int SuccessfulActions { get; set; }

        [JsonProperty("failed_actions")]
        public int FailedActions { get; set; }

        [JsonProperty("average_response_time_ms")]
        public double AverageResponseTimeMs { get; set; }

        [JsonProperty("actions_by_type")]
        public Dictionary<string, int> ActionsByType { get; set; }
    }

    /// <summary>
    /// Persian: ????? ??????
    /// English: Performance report model
    /// </summary>
    public class PerformanceReportDto
    {
        [JsonProperty("period_start")]
        public DateTimeOffset PeriodStart { get; set; }

        [JsonProperty("period_end")]
        public DateTimeOffset PeriodEnd { get; set; }

        [JsonProperty("total_stats")]
        public StatisticsDto TotalStats { get; set; }

        [JsonProperty("daily_stats")]
        public List<DailyStatsDto> DailyStats { get; set; }

        [JsonProperty("hourly_analytics")]
        public List<TimeAnalyticsDto> HourlyAnalytics { get; set; }
    }
}