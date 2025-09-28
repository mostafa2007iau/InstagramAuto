using System;
using System.Collections.Generic;

namespace InstagramAuto.Client.Models
{
    public class ChallengeStateDto
    {
        public string SessionId { get; set; }
        public string Type { get; set; }
        public Dictionary<string, object> Payload { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
