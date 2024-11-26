using Microsoft.EntityFrameworkCore;
using WebhookSecurity.Configuration;
using WebhookSecurity.Data;
using WebhookSecurity.Middleware;
using WebhookSecurity.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.Configure<WebhookSecurityOptions>(
    builder.Configuration.GetSection("WebhookSecurity"));

builder.Services.AddDbContext<WebhookSecurityContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WebhookSecurity")));

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IWebhookTokenService, WebhookTokenService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Add webhook security middleware
app.UseMiddleware<WebhookAuthenticationMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.MapControllers();

app.Run();
