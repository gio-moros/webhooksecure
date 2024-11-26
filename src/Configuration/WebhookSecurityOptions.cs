using System;

namespace WebhookSecurity.Configuration
{
    public class WebhookSecurityOptions
    {
        public TimeSpan DefaultTokenExpiration { get; set; } = TimeSpan.FromDays(30);
        public string TokenHashingSalt { get; set; }
        public int MaxTokensPerClient { get; set; } = 5;
        public TimeSpan RateLimitWindow { get; set; } = TimeSpan.FromMinutes(1);
        public int MaxRequestsPerWindow { get; set; } = 100;
    }
}
