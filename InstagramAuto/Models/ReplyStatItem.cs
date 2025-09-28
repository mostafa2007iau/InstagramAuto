using System;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian:
    ///   ??? ???? ?????? ???? ?? ???.
    /// English:
    ///   Model for reply stats per post.
    /// </summary>
    public class ReplyStatItem
    {
        public string PostId { get; set; }
        public string PostCaption { get; set; }
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
    }
}
