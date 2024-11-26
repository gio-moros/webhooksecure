using System;
using System.Threading.Tasks;
using WebhookSecurity.Models;

namespace WebhookSecurity.Services
{
    public interface IWebhookTokenService
    {
        /// <summary>
        /// Generates a new webhook token for a client
        /// </summary>
        Task<(string Token, WebhookToken TokenInfo)> GenerateTokenAsync(Guid clientId, TimeSpan? expiration = null);
        
        /// <summary>
        /// Validates a webhook token
        /// </summary>
        Task<(bool IsValid, WebhookToken TokenInfo)> ValidateTokenAsync(string token);
        
        /// <summary>
        /// Revokes a webhook token
        /// </summary>
        Task RevokeTokenAsync(Guid tokenId);
        
        /// <summary>
        /// Refreshes an existing token with a new expiration
        /// </summary>
        Task<(string NewToken, WebhookToken TokenInfo)> RefreshTokenAsync(string currentToken, TimeSpan? newExpiration = null);
        
        /// <summary>
        /// Records token usage for rate limiting and audit purposes
        /// </summary>
        Task LogTokenUsageAsync(Guid tokenId, string ipAddress, string endpointPath, bool isSuccessful, string errorMessage = null);
    }
}
