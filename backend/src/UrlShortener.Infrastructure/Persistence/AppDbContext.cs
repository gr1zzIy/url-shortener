using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Infrastructure.Auth;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Persistence;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    public DbSet<ClickEvent> ClickEvents => Set<ClickEvent>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ShortUrl>(e =>
        {
            e.HasKey(x => x.Id);
            
            e.Property(x => x.UserId)
                .IsRequired();
            
            e.Property(x => x.ShortCode)
                .HasMaxLength(32)
                .IsRequired();
            
            e.Property(x => x.OriginalUrl)
                .HasMaxLength(2048)
                .IsRequired();
            
            e.Property(x => x.IsActive)
                .HasDefaultValue(true);
            
            e.HasIndex(x => x.ShortCode)
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");

            e.HasIndex(x => x.UserId);
        });
        
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            
            e.Property(x => x.UserId)
                .IsRequired();
            
            e.Property(x => x.TokenHash)
                .HasMaxLength(128)
                .IsRequired();
            
            e.HasIndex(x => x.TokenHash)
                .IsUnique();
            
            e.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<ClickEvent>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.ShortUrlId)
                .IsRequired();

            e.Property(x => x.VisitorHash)
                .HasMaxLength(64)
                .IsRequired();

            e.Property(x => x.IpAddress)
                .HasMaxLength(64);

            e.Property(x => x.UserAgent)
                .HasMaxLength(512);

            e.Property(x => x.DeviceType)
                .HasMaxLength(32);

            e.Property(x => x.Os)
                .HasMaxLength(64);

            e.Property(x => x.Browser)
                .HasMaxLength(64);

            e.Property(x => x.CountryCode)
                .HasMaxLength(2);

            e.HasIndex(x => x.ShortUrlId);
            e.HasIndex(x => new { x.ShortUrlId, x.OccurredAt });
            e.HasIndex(x => new { x.ShortUrlId, x.VisitorHash });
        });

    }
}