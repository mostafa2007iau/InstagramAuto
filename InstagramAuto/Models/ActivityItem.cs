using System;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian:
    ///   ??? ?????? ???? ???? ????? ?? ???? LiveActivity.
    /// English:
    ///   Model for live activity to display in LiveActivityPage.
    /// </summary>
    public class ActivityItem
    {
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
