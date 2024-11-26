using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebhookSecurity.Models;
using WebhookSecurity.Data;
using WebhookSecurity.Configuration;

namespace WebhookSecurity.Services
{
    public class WebhookTokenService : IWebhookTokenService
    {
        private readonly WebhookSecurityContext _context;
        private readonly WebhookSecurityOptions _options;
        
        public WebhookTokenService(
            WebhookSecurityContext context,
            IOptions<WebhookSecurityOptions> options)
        {
            _context = context;
            _options = options.Value;
        }

        public async Task<(string Token, WebhookToken TokenInfo)> GenerateTokenAsync(Guid clientId, TimeSpan? expiration = null)
        {
            // Verify client exists and is active
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.ClientId == clientId && c.IsActive);
                
            if (client == null)
                throw new InvalidOperationException("Client not found or inactive");

            // Generate cryptographically secure token
            var tokenBytes = new byte[32]; // 256 bits
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(tokenBytes);
            }
            
            var token = Convert.ToBase64String(tokenBytes);
            
            // Hash token for storage
            var tokenHash = HashToken(token);
            
            // Create token record
            var tokenInfo = new WebhookToken
            {
                ClientId = clientId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.Add(expiration ?? _options.DefaultTokenExpiration),
                CreatedAt = DateTime.UtcNow
            };
            
            _context.WebhookTokens.Add(tokenInfo);
            await _context.SaveChangesAsync();
            
            return (token, tokenInfo);
        }

        public async Task<(bool IsValid, WebhookToken TokenInfo)> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return (false, null);
                
            var tokenHash = HashToken(token);
            
            var tokenInfo = await _context.WebhookTokens
                .Include(t => t.Client)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
                
            if (tokenInfo == null)
                return (false, null);
                
            var isValid = !tokenInfo.IsRevoked &&
                         tokenInfo.ExpiresAt > DateTime.UtcNow &&
                         tokenInfo.Client.IsActive;
                         
            if (isValid)
            {
                tokenInfo.LastUsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            
            return (isValid, tokenInfo);
        }

        public async Task RevokeTokenAsync(Guid tokenId)
        {
            var token = await _context.WebhookTokens.FindAsync(tokenId);
            if (token == null)
                throw new InvalidOperationException("Token not found");
                
            token.IsRevoked = true;
            await _context.SaveChangesAsync();
        }

        public async Task<(string NewToken, WebhookToken TokenInfo)> RefreshTokenAsync(
            string currentToken,
            TimeSpan? newExpiration = null)
        {
            var (isValid, tokenInfo) = await ValidateTokenAsync(currentToken);
            if (!isValid)
                throw new InvalidOperationException("Invalid or expired token");
                
            // Revoke current token
            await RevokeTokenAsync(tokenInfo.TokenId);
            
            // Generate new token
            return await GenerateTokenAsync(tokenInfo.ClientId, newExpiration);
        }

        public async Task LogTokenUsageAsync(
            Guid tokenId,
            string ipAddress,
            string endpointPath,
            bool isSuccessful,
            string errorMessage = null)
        {
            var usage = new TokenUsageLog
            {
                TokenId = tokenId,
                IpAddress = ipAddress,
                EndpointPath = endpointPath,
                IsSuccessful = isSuccessful,
                ErrorMessage = errorMessage,
                UsedAt = DateTime.UtcNow
            };
            
            _context.TokenUsageLogs.Add(usage);
            await _context.SaveChangesAsync();
        }

        private string HashToken(string token)
        {
            using (var sha512 = SHA512.Create())
            {
                var tokenBytes = Encoding.UTF8.GetBytes(token + _options.TokenHashingSalt);
                var hashBytes = sha512.ComputeHash(tokenBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
