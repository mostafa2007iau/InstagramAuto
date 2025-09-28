using System;

namespace InstagramAuto.Client.Models
{
    public class RuleDto
    {
        public int Id { get; set; }
        public string AccountId { get; set; }
        public string Name { get; set; }
        public string Expression { get; set; }
        public bool Enabled { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
