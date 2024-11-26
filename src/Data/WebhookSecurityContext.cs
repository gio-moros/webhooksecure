using Microsoft.EntityFrameworkCore;
using WebhookSecurity.Models;

namespace WebhookSecurity.Data
{
    public class WebhookSecurityContext : DbContext
    {
        public WebhookSecurityContext(DbContextOptions<WebhookSecurityContext> options)
            : base(options)
        {
        }
        
        public DbSet<Client> Clients { get; set; }
        public DbSet<WebhookToken> WebhookTokens { get; set; }
        public DbSet<TokenUsageLog> TokenUsageLogs { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Client entity
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.ClientId);
                entity.Property(e => e.ClientName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("GETUTCDATE()");
            });
            
            // Configure WebhookToken entity
            modelBuilder.Entity<WebhookToken>(entity =>
            {
                entity.HasKey(e => e.TokenId);
                entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(512);
                entity.Property(e => e.IsRevoked).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasOne(e => e.Client)
                    .WithMany(c => c.Tokens)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configure TokenUsageLog entity
            modelBuilder.Entity<TokenUsageLog>(entity =>
            {
                entity.HasKey(e => e.LogId);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.EndpointPath).IsRequired().HasMaxLength(256);
                entity.Property(e => e.UsedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasOne<WebhookToken>()
                    .WithMany()
                    .HasForeignKey(e => e.TokenId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
