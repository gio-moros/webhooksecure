using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using WebhookSecurity.Configuration;
using WebhookSecurity.Models;

namespace WebhookSecurity.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly WebhookSecurityOptions _options;
        
        public RateLimitingMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            IOptions<WebhookSecurityOptions> options)
        {
            _next = next;
            _cache = cache;
            _options = options.Value;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Items["WebhookToken"] as WebhookToken;
            if (token == null)
            {
                await _next(context);
                return;
            }
            
            var cacheKey = $"rate_limit_{token.TokenId}";
            var requestCount = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _options.RateLimitWindow;
                return 0;
            });
            
            if (requestCount >= _options.MaxRequestsPerWindow)
            {
                context.Response.StatusCode = 429; // Too Many Requests
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Rate limit exceeded",
                    retryAfter = _options.RateLimitWindow.TotalSeconds
                });
                return;
            }
            
            _cache.Set(cacheKey, requestCount + 1, _options.RateLimitWindow);
            
            await _next(context);
        }
    }
}
