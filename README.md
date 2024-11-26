# Webhook Security Implementation

A comprehensive webhook security implementation using C# .NET and SQL Server, providing secure token management, validation, and rate limiting capabilities.

## Features

- 🔐 Secure token generation and management
- 🔒 Token hashing with salt using SHA512
- ⏰ Token expiration mechanism
- 🚦 Rate limiting per token
- 📝 Comprehensive request logging
- 🛡️ Prevention of token replay attacks
- 🎯 Client-specific token management
- 📊 Usage tracking and analytics

## Prerequisites

- .NET 6.0 or later
- SQL Server 2019 or later
- Visual Studio 2022 or any compatible IDE

## Project Structure

```
WebhookSecurity/
├── src/
│   ├── Configuration/
│   │   └── WebhookSecurityOptions.cs
│   ├── Data/
│   │   └── WebhookSecurityContext.cs
│   ├── Database/
│   │   └── CreateWebhookSecurityTables.sql
│   ├── Middleware/
│   │   ├── WebhookAuthenticationMiddleware.cs
│   │   └── RateLimitingMiddleware.cs
│   ├── Models/
│   │   ├── Client.cs
│   │   ├── WebhookToken.cs
│   │   └── TokenUsageLog.cs
│   └── Services/
│       ├── IWebhookTokenService.cs
│       └── WebhookTokenService.cs
```

## Setup Instructions

1. Clone the repository:
```bash
git clone <repository-url>
cd WebhookSecurity
```

2. Create the database:
- Open SQL Server Management Studio
- Create a new database named 'WebhookSecurity'
- Execute the script in `src/Database/CreateWebhookSecurityTables.sql`

3. Configure the application:
- Create or modify `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "WebhookSecurity": "Server=your_server;Database=WebhookSecurity;Trusted_Connection=True;"
  },
  "WebhookSecurity": {
    "DefaultTokenExpiration": "30.00:00:00",
    "TokenHashingSalt": "your-secure-salt-here",
    "MaxTokensPerClient": 5,
    "RateLimitWindow": "00:01:00",
    "MaxRequestsPerWindow": 100
  }
}
```

4. Install NuGet packages:
```bash
dotnet restore
```

5. Build the project:
```bash
dotnet build
```

## Usage

### 1. Register Services

In your `Startup.cs` or `Program.cs`:

```csharp
builder.Services.Configure<WebhookSecurityOptions>(
    builder.Configuration.GetSection("WebhookSecurity"));

builder.Services.AddDbContext<WebhookSecurityContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WebhookSecurity")));

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IWebhookTokenService, WebhookTokenService>();

// In app configuration
app.UseMiddleware<WebhookAuthenticationMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
```

### 2. Generate Tokens

```csharp
public class WebhookController : ControllerBase
{
    private readonly IWebhookTokenService _tokenService;

    public WebhookController(IWebhookTokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateToken(Guid clientId)
    {
        var (token, info) = await _tokenService.GenerateTokenAsync(clientId);
        return Ok(new { token, expiresAt = info.ExpiresAt });
    }
}
```

### 3. Use Tokens in Webhook Requests

Include the token in the request header:

```http
POST /api/webhook
Host: your-api.com
X-Webhook-Token: your-token-here
Content-Type: application/json

{
    "event": "user.created",
    "data": {
        // webhook payload
    }
}
```

## Security Considerations

1. **Token Storage**: Never store raw tokens. Only store hashed versions.
2. **Salt Management**: Keep your token hashing salt secure and unique per environment.
3. **Rate Limiting**: Adjust rate limiting based on your specific needs.
4. **Token Expiration**: Set appropriate token expiration times.
5. **Logging**: Monitor token usage logs for suspicious activities.

## Error Handling

The middleware provides standard HTTP status codes:
- 401: Invalid or missing token
- 429: Rate limit exceeded
- 500: Server error

Example error response:
```json
{
    "error": "Rate limit exceeded",
    "retryAfter": 60
}
```

## Monitoring and Maintenance

1. Regular monitoring:
   - Check token usage patterns
   - Monitor rate limit hits
   - Review failed authentication attempts

2. Database maintenance:
   - Regular cleanup of expired tokens
   - Index maintenance
   - Log rotation

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
