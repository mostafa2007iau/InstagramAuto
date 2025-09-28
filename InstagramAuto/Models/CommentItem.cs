using System;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian:
    ///   ??? ????? ???? ????? ? ?????? ????????.
    /// English:
    ///   Model for displaying and managing comments.
    /// </summary>
    public class CommentItem
    {
        public string Id { get; set; }
        public string MediaId { get; set; }
        public string User { get; set; }
        public string UserId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
