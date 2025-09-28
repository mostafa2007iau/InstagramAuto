using System;
using System.Collections.Generic;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian:
    ///   ??? ????? ???? ?????? ? ?????? ?????? ?????? ? ??????.
    /// English:
    ///   Model for editing and managing reply/DM rules.
    /// </summary>
    public class RuleItem
    {
        public int? Id { get; set; }
        public string AccountId { get; set; }
        public string MediaId { get; set; }
        public string Name { get; set; }
        public string Condition { get; set; }
        public List<string> Replies { get; set; }
        public bool SendDM { get; set; }
        public List<string> DMs { get; set; }
        public bool Enabled { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
