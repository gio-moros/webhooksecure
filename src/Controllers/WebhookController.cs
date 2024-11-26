using Microsoft.AspNetCore.Mvc;
using WebhookSecurity.Models;
using WebhookSecurity.Services;

namespace WebhookSecurity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IWebhookTokenService _tokenService;

        public WebhookController(IWebhookTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("token/generate")]
        public async Task<IActionResult> GenerateToken(Guid clientId)
        {
            try
            {
                var (token, info) = await _tokenService.GenerateTokenAsync(clientId);
                return Ok(new { token, expiresAt = info.ExpiresAt });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("token/refresh")]
        public async Task<IActionResult> RefreshToken([FromHeader(Name = "X-Webhook-Token")] string currentToken)
        {
            try
            {
                var (newToken, info) = await _tokenService.RefreshTokenAsync(currentToken);
                return Ok(new { token = newToken, expiresAt = info.ExpiresAt });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("token/revoke")]
        public async Task<IActionResult> RevokeToken(Guid tokenId)
        {
            try
            {
                await _tokenService.RevokeTokenAsync(tokenId);
                return Ok(new { message = "Token revoked successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("webhook")]
        public IActionResult ReceiveWebhook([FromBody] object payload)
        {
            // The WebhookAuthenticationMiddleware will handle token validation
            // If we reach here, the token is valid
            return Ok(new { message = "Webhook received successfully" });
        }
    }
}
