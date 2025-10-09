namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ???????? ????
    /// English: Session locale
    /// </summary>
    public static class SessionCreateInLocale
    {
        public const string En = "en";
        public const string Fa = "fa";
    }

    /// <summary>
    /// Persian: ????????? ???
    /// English: Job statuses
    /// </summary>
    public static class JobStatuses
    {
        public const string Queued = "queued";
        public const string Processing = "processing";
        public const string Completed = "completed";
        public const string Failed = "failed";
        public const string Cancelled = "cancelled";
        public const string Retrying = "retrying";
        public const string MovedToDlq = "moved_to_dlq";
    }

    /// <summary>
    /// Persian: ????? ??????
    /// English: Activity types
    /// </summary>
    public static class ActivityTypes
    {
        public const string Comment = "comment";
        public const string Like = "like";
        public const string Follow = "follow";
        public const string Unfollow = "unfollow";
        public const string DirectMessage = "direct_message";
        public const string StoryView = "story_view";
        public const string PostView = "post_view";
    }

    /// <summary>
    /// Persian: ???? ?????
    /// English: Notification levels
    /// </summary>
    public static class NotificationLevels
    {
        public const string Info = "info";
        public const string Warning = "warning";
        public const string Error = "error";
        public const string Success = "success";
    }

    /// <summary>
    /// Persian: ????????? ?????
    /// English: Health statuses
    /// </summary>
    public static class HealthStatuses
    {
        public const string Healthy = "healthy";
        public const string Degraded = "degraded";
        public const string Unhealthy = "unhealthy";
    }
}