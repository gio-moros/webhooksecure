using System;
using System.Collections.Generic;

namespace WebhookSecurity.Models
{
    public class Client
    {
        public Guid ClientId { get; set; }
        public string ClientName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
        
        // Navigation property
        public virtual ICollection<WebhookToken> Tokens { get; set; }
    }
}
