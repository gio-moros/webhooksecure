using System;

namespace WebhookSecurity.Models
{
    public class TokenUsageLog
    {
        public long LogId { get; set; }
        public Guid TokenId { get; set; }
        public string IpAddress { get; set; }
        public string EndpointPath { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime UsedAt { get; set; }
    }
}
