using Microsoft.EntityFrameworkCore;
using MinnieWorld.Web.Models;

namespace MinnieWorld.Web.Data;

/// <summary>
/// One database for the whole site. Each section owns a SQL schema (core, astro, blog),
/// so a section can later move to its own database by pointing its tables elsewhere.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    public DbSet<HoroscopeCache> HoroscopeCaches => Set<HoroscopeCache>();
    public DbSet<ConsultationRequest> Consultations => Set<ConsultationRequest>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Post> Posts => Set<Post>();

    public DbSet<SiteSetting> Settings => Set<SiteSetting>();
    public DbSet<SiteCounter> Counters => Set<SiteCounter>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();
    public DbSet<ExamPurchase> ExamPurchases => Set<ExamPurchase>();

    /// <summary>Every DateTime is stored in UTC. Reading it back as UTC makes the API send a trailing "Z", so browsers show local time correctly.</summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcDateTimeConverter>();
    }

    public sealed class UtcDateTimeConverter() : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
        v => v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : DateTime.SpecifyKind(v, DateTimeKind.Utc),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Site>(e =>
        {
            e.ToTable("Sites", "core");
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Tagline).HasMaxLength(300);
            e.Property(x => x.Path).HasMaxLength(300);
            e.Property(x => x.ThumbnailUrl).HasMaxLength(500);
            e.Property(x => x.AccentColor).HasMaxLength(20);
        });

        modelBuilder.Entity<AdminUser>(e =>
        {
            e.ToTable("AdminUsers", "core");
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.DisplayName).HasMaxLength(100);
        });

        modelBuilder.Entity<ContactMessage>(e =>
        {
            e.ToTable("ContactMessages", "core");
            e.Property(x => x.SiteKey).HasMaxLength(20);
            e.Property(x => x.FullName).HasMaxLength(120);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.Subject).HasMaxLength(200);
            e.Property(x => x.Message).HasMaxLength(4000);
        });

        modelBuilder.Entity<HoroscopeCache>(e =>
        {
            e.ToTable("HoroscopeCache", "astro");
            e.HasIndex(x => new { x.Sign, x.Period, x.HoroscopeDate }).IsUnique();
            e.Property(x => x.Sign).HasMaxLength(20);
            e.Property(x => x.Period).HasMaxLength(10);
            e.Property(x => x.Source).HasMaxLength(50);
        });

        modelBuilder.Entity<ConsultationRequest>(e =>
        {
            e.ToTable("Consultations", "astro");
            e.Property(x => x.FullName).HasMaxLength(120);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.BirthPlace).HasMaxLength(200);
            e.Property(x => x.ServiceType).HasMaxLength(100);
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.Property(x => x.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Testimonial>(e =>
        {
            e.ToTable("Testimonials", "astro");
            e.Property(x => x.CustomerName).HasMaxLength(120);
            e.Property(x => x.Location).HasMaxLength(120);
            e.Property(x => x.Quote).HasMaxLength(1000);
            e.Property(x => x.AvatarUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<Course>(e =>
        {
            e.ToTable("Courses", "blog");
            e.HasIndex(x => new { x.Blog, x.Slug }).IsUnique();
            e.Property(x => x.Blog).HasMaxLength(20);
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Slug).HasMaxLength(220);
            e.Property(x => x.Subtitle).HasMaxLength(400);
            e.Property(x => x.Category).HasMaxLength(80);
            e.Property(x => x.Level).HasMaxLength(30);
            e.Property(x => x.ThumbnailUrl).HasMaxLength(500);
            e.HasMany(x => x.Lessons).WithOne(x => x.Course!).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Lesson>(e =>
        {
            e.ToTable("Lessons", "blog");
            e.HasIndex(x => new { x.CourseId, x.Slug }).IsUnique();
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Slug).HasMaxLength(220);
            e.Property(x => x.Summary).HasMaxLength(500);
            e.Property(x => x.VideoUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<Post>(e =>
        {
            e.ToTable("Posts", "blog");
            e.HasIndex(x => new { x.Blog, x.Slug }).IsUnique();
            e.HasIndex(x => new { x.Blog, x.IsPublished, x.PublishedAt });
            e.Property(x => x.Blog).HasMaxLength(20);
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Slug).HasMaxLength(220);
            e.Property(x => x.Excerpt).HasMaxLength(500);
            e.Property(x => x.Category).HasMaxLength(80);
            e.Property(x => x.Tags).HasMaxLength(300);
            e.Property(x => x.CoverImageUrl).HasMaxLength(500);
            e.Property(x => x.Author).HasMaxLength(120);
        });

        modelBuilder.Entity<SiteCounter>(e =>
        {
            e.ToTable("Counters", "core");
            e.HasKey(x => x.Key);
            e.Property(x => x.Key).HasMaxLength(50);
        });

        modelBuilder.Entity<SiteSetting>(e =>
        {
            e.ToTable("Settings", "core");
            e.HasKey(x => x.Key);
            e.Property(x => x.Key).HasMaxLength(100);
            e.Property(x => x.Value).HasMaxLength(2000);
        });

        modelBuilder.Entity<Student>(e =>
        {
            e.ToTable("Students", "exam");
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.FullName).HasMaxLength(120);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.SecurityStamp).HasMaxLength(64);
        });

        modelBuilder.Entity<Exam>(e =>
        {
            e.ToTable("Exams", "exam");
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Slug).HasMaxLength(220);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.Category).HasMaxLength(100);
            e.Property(x => x.Kind).HasMaxLength(30);
            e.Property(x => x.NegativeMark).HasPrecision(4, 2);
            e.HasMany(x => x.Questions).WithOne(x => x.Exam!).HasForeignKey(x => x.ExamId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(e =>
        {
            e.ToTable("Questions", "exam");
            e.HasIndex(x => new { x.ExamId, x.SortOrder });
            e.Property(x => x.Section).HasMaxLength(100);
            e.Property(x => x.OptionA).HasMaxLength(1000);
            e.Property(x => x.OptionB).HasMaxLength(1000);
            e.Property(x => x.OptionC).HasMaxLength(1000);
            e.Property(x => x.OptionD).HasMaxLength(1000);
            e.Property(x => x.Explanation).HasMaxLength(2000);
            e.Property(x => x.OptionAHi).HasMaxLength(1000);
            e.Property(x => x.OptionBHi).HasMaxLength(1000);
            e.Property(x => x.OptionCHi).HasMaxLength(1000);
            e.Property(x => x.OptionDHi).HasMaxLength(1000);
            e.Property(x => x.ExplanationHi).HasMaxLength(2000);
            e.Property(x => x.Marks).HasPrecision(5, 2).HasDefaultValue(1m);
            e.Ignore(x => x.Options);
            e.Ignore(x => x.OptionsHi);
        });

        modelBuilder.Entity<ExamAttempt>(e =>
        {
            e.ToTable("Attempts", "exam");
            e.HasIndex(x => new { x.StudentId, x.StartedAt });
            e.HasIndex(x => new { x.ExamId, x.SubmittedAt });
            e.Property(x => x.Score).HasPrecision(7, 2);
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Exam).WithMany().HasForeignKey(x => x.ExamId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExamPurchase>(e =>
        {
            e.ToTable("Purchases", "exam");
            e.HasIndex(x => new { x.StudentId, x.ExamId });
            e.HasIndex(x => new { x.Status, x.CreatedAt });
            // One UPI payment can only unlock one test; a rejected claim frees the id for a corrected resubmission.
            e.HasIndex(x => x.UpiRef).IsUnique().HasFilter("[Status] <> 'Rejected'");
            e.Property(x => x.UpiRef).HasMaxLength(30);
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.Note).HasMaxLength(500);
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Exam).WithMany().HasForeignKey(x => x.ExamId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
