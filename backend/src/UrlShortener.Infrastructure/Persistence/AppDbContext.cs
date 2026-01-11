using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<ShortUrl>();
        e.HasKey(x => x.Id);

        e.Property(x => x.ShortCode).HasMaxLength(32).IsRequired();
        e.Property(x => x.OriginalUrl).HasMaxLength(2048).IsRequired();

        e.HasIndex(x => x.ShortCode).IsUnique();
    }
}
