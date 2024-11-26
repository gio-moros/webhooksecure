using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using WebhookSecurity.Services;
using WebhookSecurity.Configuration;

namespace WebhookSecurity.Middleware
{
    public class WebhookAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebhookTokenService _tokenService;
        private readonly WebhookSecurityOptions _options;
        
        public WebhookAuthenticationMiddleware(
            RequestDelegate next,
            IWebhookTokenService tokenService,
            IOptions<WebhookSecurityOptions> options)
        {
            _next = next;
            _tokenService = tokenService;
            _options = options.Value;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["X-Webhook-Token"].ToString();
            
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Webhook token is required" });
                return;
            }
            
            var (isValid, tokenInfo) = await _tokenService.ValidateTokenAsync(token);
            
            if (!isValid)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid or expired webhook token" });
                return;
            }
            
            // Add token info to the HttpContext for later use
            context.Items["WebhookToken"] = tokenInfo;
            
            try
            {
                await _next(context);
                
                // Log successful request
                await _tokenService.LogTokenUsageAsync(
                    tokenInfo.TokenId,
                    context.Connection.RemoteIpAddress?.ToString(),
                    context.Request.Path,
                    true);
            }
            catch (Exception ex)
            {
                // Log failed request
                await _tokenService.LogTokenUsageAsync(
                    tokenInfo.TokenId,
                    context.Connection.RemoteIpAddress?.ToString(),
                    context.Request.Path,
                    false,
                    ex.Message);
                    
                throw;
            }
        }
    }
}
