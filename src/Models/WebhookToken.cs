using System;

namespace WebhookSecurity.Models
{
    public class WebhookToken
    {
        public Guid TokenId { get; set; }
        public Guid ClientId { get; set; }
        public string TokenHash { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        
        // Navigation property
        public virtual Client Client { get; set; }
    }
}
