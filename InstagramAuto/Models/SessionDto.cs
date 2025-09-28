using System;

namespace InstagramAuto.Client.Models
{
    public class SessionDto
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string Proxy { get; set; }
        public bool ProxyEnabled { get; set; }
        public string SessionBlob { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
