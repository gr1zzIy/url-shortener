using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Infrastructure.Auth;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Persistence;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var e = modelBuilder.Entity<ShortUrl>();

        e.HasKey(x => x.Id);

        e.Property(x => x.UserId).IsRequired();

        e.Property(x => x.ShortCode)
            .HasMaxLength(32)
            .IsRequired();

        e.Property(x => x.OriginalUrl)
            .HasMaxLength(2048)
            .IsRequired();

        e.Property(x => x.IsActive).HasDefaultValue(true);

        e.HasIndex(x => x.ShortCode)
            .IsUnique()
            .HasFilter("\"DeletedAt\" IS NULL");

        e.HasIndex(x => x.UserId);
    }
}