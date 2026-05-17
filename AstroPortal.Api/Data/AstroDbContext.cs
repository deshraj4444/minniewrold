using AstroPortal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AstroPortal.Api.Data;

public class AstroDbContext : DbContext
{
    public AstroDbContext(DbContextOptions<AstroDbContext> options) : base(options)
    {
    }

    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
    public DbSet<KundaliRequest> KundaliRequests => Set<KundaliRequest>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<HoroscopeCache> HoroscopeCaches => Set<HoroscopeCache>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogPost>()
            .HasIndex(blog => blog.Slug)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .Property(product => product.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<HoroscopeCache>()
            .HasIndex(cache => new { cache.Sign, cache.HoroscopeDate })
            .IsUnique();
    }
}
