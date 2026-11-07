using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MinnieWorld.Web.Infrastructure;
using MinnieWorld.Web.Models;

namespace MinnieWorld.Web.Data;

public static class DbSeeder
{
    public static async Task MigrateAndSeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbSeeder));

        await db.Database.MigrateAsync();

        await SeedAdminAsync(db, config, scope.ServiceProvider.GetRequiredService<IPasswordHasher<AdminUser>>(), logger);
        await SeedSitesAsync(db);
        await SeedTestimonialsAsync(db);
        foreach (var blog in BlogKeys.All)
        {
            await SeedBlogAsync(db, blog, Path.Combine(env.ContentRootPath, "Data", "Seed", $"{blog}.json"), logger);
        }

        await SeedExamsAsync(db, Path.Combine(env.ContentRootPath, "Data", "Seed", "exams"), logger);
        foreach (var blog in BlogKeys.All)
        {
            await SeedPostLibraryAsync(db, blog, Path.Combine(env.ContentRootPath, "Data", "Seed", "posts", blog), logger);
        }
    }

    private sealed record SeedPostFile(List<SeedLibraryPost> Posts);
    private sealed record SeedLibraryPost(string Slug, string Title, string? Excerpt, string Content, string Category, string? Tags, string? CoverImageUrl, string? Author);

    /// <summary>
    /// Adds posts from Data/Seed/posts/{blog}/*.json whose slug isn't in that blog yet (e.g. the Hindi mantra, stotra
    /// and temple library). Existing posts are never touched, so admin edits survive and new files appear on the next start.
    /// </summary>
    private static async Task SeedPostLibraryAsync(AppDbContext db, string blog, string folder, ILogger logger)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        var existing = (await db.Posts.Where(p => p.Blog == blog).Select(p => p.Slug).ToListAsync()).ToHashSet();
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var now = DateTime.UtcNow;
        var added = 0;
        foreach (var file in Directory.EnumerateFiles(folder, "*.json").Order())
        {
            SeedPostFile? seed;
            try
            {
                seed = JsonSerializer.Deserialize<SeedPostFile>(await File.ReadAllTextAsync(file), options);
            }
            catch (JsonException e)
            {
                logger.LogWarning("Skipped {File}: {Error}", Path.GetFileName(file), e.Message);
                continue;
            }

            foreach (var p in seed?.Posts ?? [])
            {
                var slug = Slug.Create(p.Slug);
                if (string.IsNullOrWhiteSpace(p.Title) || string.IsNullOrWhiteSpace(p.Content) || !existing.Add(slug))
                {
                    continue;
                }

                db.Posts.Add(new Post
                {
                    Blog = blog,
                    Slug = slug,
                    Title = Trunc(p.Title, 200),
                    Excerpt = Trunc(p.Excerpt ?? "", 500),
                    Content = p.Content,
                    Category = Trunc(p.Category, 80),
                    Tags = Trunc(p.Tags ?? "", 300),
                    CoverImageUrl = p.CoverImageUrl ?? "",
                    Author = Trunc(p.Author ?? "", 120),
                    // Stagger publish times so the library keeps its file order in "latest first" lists.
                    PublishedAt = now.AddMinutes(-added),
                    UpdatedAt = now
                });
                added++;
            }
        }

        if (added > 0)
        {
            await db.SaveChangesAsync();
            logger.LogInformation("Added {Count} library posts to the {Blog} blog", added, blog);
        }
    }

    private static string Trunc(string s, int max) => s.Length <= max ? s.Trim() : s[..max].Trim();

    private sealed record SeedExam(string Slug, string Title, string? Description, string Category, string? Kind, int? Year, int DurationMinutes, decimal? NegativeMark, int? Price, int? Version);

    /// <summary>
    /// Adds each test in Data/Seed/exams/*.json whose slug isn't in the database yet. An existing test is left alone
    /// (so admin edits survive) unless the file's "version" is higher than the one it was seeded from; then its details
    /// and questions are refreshed in place, keeping question ids so earlier results still line up.
    /// </summary>
    private static async Task SeedExamsAsync(AppDbContext db, string folder, ILogger logger)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        var existing = await db.Exams.ToDictionaryAsync(e => e.Slug);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        foreach (var file in Directory.EnumerateFiles(folder, "*.json").Order())
        {
            var json = await File.ReadAllTextAsync(file);
            SeedExam? meta;
            try
            {
                meta = JsonSerializer.Deserialize<SeedExam>(json, options);
            }
            catch (JsonException e)
            {
                logger.LogWarning("Skipped {File}: {Error}", Path.GetFileName(file), e.Message);
                continue;
            }

            if (meta is null || string.IsNullOrWhiteSpace(meta.Slug))
            {
                continue;
            }

            var version = Math.Max(1, meta.Version ?? 1);
            existing.TryGetValue(meta.Slug, out var exam);
            if (exam is not null && exam.SeedVersion >= version)
            {
                continue;
            }

            var parsed = Services.Exams.ExamImport.Parse(json);
            foreach (var error in parsed.Errors)
            {
                logger.LogWarning("{File}: {Error}", Path.GetFileName(file), error);
            }

            if (parsed.Questions.Count == 0)
            {
                continue;
            }

            var isNew = exam is null;
            exam ??= new Exam { Slug = meta.Slug };
            exam.Title = meta.Title;
            exam.Description = meta.Description ?? "";
            exam.Category = meta.Category;
            exam.Kind = ExamKinds.All.Contains(meta.Kind) ? meta.Kind! : ExamKinds.Mock;
            exam.Year = meta.Year;
            exam.DurationMinutes = meta.DurationMinutes > 0 ? meta.DurationMinutes : parsed.Questions.Count;
            exam.NegativeMark = meta.NegativeMark ?? 0;
            exam.Price = meta.Price ?? 0;
            exam.SeedVersion = version;
            exam.UpdatedAt = DateTime.UtcNow;

            if (isNew)
            {
                exam.Questions.AddRange(parsed.Questions.Select((q, i) => { q.SortOrder = i + 1; return q; }));
                db.Exams.Add(exam);
                existing[meta.Slug] = exam;
            }
            else
            {
                var current = await db.Questions.Where(q => q.ExamId == exam.Id).OrderBy(q => q.SortOrder).ThenBy(q => q.Id).ToListAsync();
                for (var i = 0; i < parsed.Questions.Count; i++)
                {
                    var src = parsed.Questions[i];
                    var q = i < current.Count ? current[i] : new Question { ExamId = exam.Id };
                    q.SortOrder = i + 1;
                    q.Section = src.Section;
                    q.Text = src.Text;
                    q.OptionA = src.OptionA;
                    q.OptionB = src.OptionB;
                    q.OptionC = src.OptionC;
                    q.OptionD = src.OptionD;
                    q.Answer = src.Answer;
                    q.Explanation = src.Explanation;
                    q.TextHi = src.TextHi;
                    q.OptionAHi = src.OptionAHi;
                    q.OptionBHi = src.OptionBHi;
                    q.OptionCHi = src.OptionCHi;
                    q.OptionDHi = src.OptionDHi;
                    q.ExplanationHi = src.ExplanationHi;
                    q.Marks = src.Marks;
                    q.Figure = src.Figure;
                    q.Passage = src.Passage;
                    q.PassageHi = src.PassageHi;
                    if (i >= current.Count) db.Questions.Add(q);
                }

                db.Questions.RemoveRange(current.Skip(parsed.Questions.Count));
            }

            await db.SaveChangesAsync();
            logger.LogInformation("{Action} test {Slug} (v{Version}) with {Count} questions", isNew ? "Seeded" : "Updated", meta.Slug, version, parsed.Questions.Count);
        }
    }

    /// <summary>Creates the first admin from Admin:Email / Admin:Password when no admin exists yet.</summary>
    private static async Task SeedAdminAsync(AppDbContext db, IConfiguration config, IPasswordHasher<AdminUser> hasher, ILogger logger)
    {
        if (await db.AdminUsers.AnyAsync())
        {
            return;
        }

        var email = config["Admin:Email"]?.Trim().ToLowerInvariant();
        var password = config["Admin:Password"];
        // Placeholder values (e.g. "YOUR_ADMIN_EMAIL" from appsettings.Production.json) must never become a real login.
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)
            || !email.Contains('@') || email.Contains("your_", StringComparison.OrdinalIgnoreCase)
            || password.Contains("YOUR_", StringComparison.OrdinalIgnoreCase) || password.Length < 10)
        {
            logger.LogWarning("No admin user exists. Set Admin:Email and Admin:Password (a real email and a password of at least 10 characters) in appsettings.Production.json or environment variables, and restart to create one.");
            return;
        }

        var admin = new AdminUser { Email = email, DisplayName = config["Admin:DisplayName"] ?? "Admin" };
        admin.PasswordHash = hasher.HashPassword(admin, password);
        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync();
        logger.LogInformation("Created admin user {Email}", email);
    }

    private static async Task SeedSitesAsync(AppDbContext db)
    {
        if (await db.Sites.AnyAsync())
        {
            return;
        }

        db.Sites.AddRange(
            new Site
            {
                Name = "AstroAura",
                Tagline = "Daily horoscopes, kundali consultations and Vedic astrology guidance.",
                Path = "/astro",
                ThumbnailUrl = "/covers/site-astro.png",
                AccentColor = "#d4a64a",
                SortOrder = 1
            },
            new Site
            {
                Name = "Dharma Path",
                Tagline = "Scriptures, mantras, festivals and temples: Sanatan wisdom in simple words.",
                Path = "/dharma",
                ThumbnailUrl = "/covers/site-dharma.png",
                AccentColor = "#9a3412",
                SortOrder = 2
            },
            new Site
            {
                Name = "Curious Minds",
                Tagline = "General knowledge made fun: science, India, world and exam-ready courses.",
                Path = "/gk",
                ThumbnailUrl = "/covers/site-gk.png",
                AccentColor = "#2563eb",
                SortOrder = 3
            });
        await db.SaveChangesAsync();
    }

    private static async Task SeedTestimonialsAsync(AppDbContext db)
    {
        if (await db.Testimonials.AnyAsync())
        {
            return;
        }

        db.Testimonials.AddRange(
            new Testimonial { CustomerName = "Ananya Sharma", Location = "Delhi", Quote = "The kundali consultation was thoughtful, practical and easy to understand.", Rating = 5 },
            new Testimonial { CustomerName = "Rohit Mehta", Location = "Mumbai", Quote = "Daily guidance and remedies helped me bring structure to important decisions.", Rating = 5 },
            new Testimonial { CustomerName = "Meera Kapoor", Location = "Jaipur", Quote = "Quick response and a very professional astrology experience from start to finish.", Rating = 5 });
        await db.SaveChangesAsync();
    }

    private sealed record SeedFile(List<SeedCourse> Courses, List<SeedPost> Posts);
    private sealed record SeedCourse(string Title, string Subtitle, string Description, string Category, string Level, string ThumbnailUrl, bool IsFeatured, List<SeedLesson> Lessons);
    private sealed record SeedLesson(string Title, string Summary, string Content, string? VideoUrl, int DurationMinutes);
    private sealed record SeedPost(string Title, string Excerpt, string Content, string Category, string Tags, string CoverImageUrl, string Author);

    /// <summary>Loads courses and posts from Data/Seed/{blog}.json, only into an empty blog (it never overwrites admin edits).</summary>
    private static async Task SeedBlogAsync(AppDbContext db, string blog, string path, ILogger logger)
    {
        if (await db.Courses.AnyAsync(c => c.Blog == blog) || await db.Posts.AnyAsync(p => p.Blog == blog) || !File.Exists(path))
        {
            return;
        }

        var seed = JsonSerializer.Deserialize<SeedFile>(await File.ReadAllTextAsync(path), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        if (seed is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var (c, ci) in seed.Courses.Select((c, i) => (c, i)))
        {
            var course = new Course
            {
                Blog = blog,
                Title = c.Title,
                Slug = Slug.Create(c.Title),
                Subtitle = c.Subtitle,
                Description = c.Description,
                Category = c.Category,
                Level = c.Level,
                ThumbnailUrl = c.ThumbnailUrl,
                IsFeatured = c.IsFeatured,
                CreatedAt = now.AddMinutes(-ci),
                UpdatedAt = now.AddMinutes(-ci)
            };
            course.Lessons.AddRange(c.Lessons.Select((l, li) => new Lesson
            {
                Title = l.Title,
                Slug = Slug.Create(l.Title),
                Summary = l.Summary,
                Content = l.Content,
                VideoUrl = l.VideoUrl ?? "",
                DurationMinutes = l.DurationMinutes,
                SortOrder = li + 1
            }));
            db.Courses.Add(course);
        }

        foreach (var (p, pi) in seed.Posts.Select((p, i) => (p, i)))
        {
            db.Posts.Add(new Post
            {
                Blog = blog,
                Title = p.Title,
                Slug = Slug.Create(p.Title),
                Excerpt = p.Excerpt,
                Content = p.Content,
                Category = p.Category,
                Tags = p.Tags,
                CoverImageUrl = p.CoverImageUrl,
                Author = p.Author,
                PublishedAt = now.AddDays(-pi * 3),
                UpdatedAt = now.AddDays(-pi * 3)
            });
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Courses} courses and {Posts} posts into the {Blog} blog", seed.Courses.Count, seed.Posts.Count, blog);
    }
}
