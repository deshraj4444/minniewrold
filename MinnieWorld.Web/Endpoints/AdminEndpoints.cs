using Microsoft.EntityFrameworkCore;
using MinnieWorld.Web.Contracts;
using MinnieWorld.Web.Data;
using MinnieWorld.Web.Infrastructure;
using MinnieWorld.Web.Models;

namespace MinnieWorld.Web.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/api/admin")
            .RequireAuthorization()
            .AddEndpointFilter<RequireCsrfHeaderFilter>();

        admin.MapGet("/stats", async (AppDbContext db) =>
        {
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            return new
            {
                Courses = await db.Courses.CountAsync(),
                Lessons = await db.Lessons.CountAsync(),
                Posts = await db.Posts.CountAsync(),
                NewConsultations = await db.Consultations.CountAsync(k => k.Status == ConsultationStatus.New),
                UnreadMessages = await db.ContactMessages.CountAsync(m => !m.IsRead),
                Testimonials = await db.Testimonials.CountAsync(),
                Exams = await db.Exams.CountAsync(),
                Students = await db.Students.CountAsync(),
                AttemptsToday = await db.ExamAttempts.CountAsync(a => a.StartedAt >= DateTime.UtcNow.Date),
                PendingPayments = await db.ExamPurchases.CountAsync(p => p.Status == PaymentStatus.Pending),
                RevenueThisMonth = await db.ExamPurchases
                    .Where(p => p.Status == PaymentStatus.Approved && p.ReviewedAt >= monthStart)
                    .SumAsync(p => (int?)p.Amount) ?? 0
            };
        });

        admin.MapAdminExams();

        MapSites(admin.MapGroup("/sites"));
        MapTestimonials(admin.MapGroup("/testimonials"));
        MapConsultations(admin.MapGroup("/consultations"));
        MapMessages(admin.MapGroup("/messages"));
        MapCourses(admin);
        MapPosts(admin.MapGroup("/posts"));
        MapUploads(admin);
    }

    private static void MapSites(RouteGroupBuilder g)
    {
        g.MapGet("/", async (AppDbContext db) => await db.Sites.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync());

        g.MapPost("/", async (SiteRequest req, AppDbContext db) =>
        {
            var site = new Site();
            Apply(site, req);
            db.Sites.Add(site);
            await db.SaveChangesAsync();
            return Results.Ok(site);
        }).Validate<SiteRequest>();

        g.MapPut("/{id:int}", async (int id, SiteRequest req, AppDbContext db) =>
        {
            var site = await db.Sites.FindAsync(id);
            if (site is null) return Results.NotFound();
            Apply(site, req);
            await db.SaveChangesAsync();
            return Results.Ok(site);
        }).Validate<SiteRequest>();

        g.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
            await db.Sites.Where(s => s.Id == id).ExecuteDeleteAsync() == 0 ? Results.NotFound() : Results.NoContent());

        static void Apply(Site s, SiteRequest r)
        {
            s.Name = r.Name.Trim();
            s.Tagline = r.Tagline.Trim();
            s.Path = r.Path.Trim();
            s.ThumbnailUrl = r.ThumbnailUrl.Trim();
            s.AccentColor = r.AccentColor.Trim();
            s.SortOrder = r.SortOrder;
            s.IsActive = r.IsActive;
        }
    }

    private static void MapTestimonials(RouteGroupBuilder g)
    {
        g.MapGet("/", async (AppDbContext db) => await db.Testimonials.AsNoTracking().OrderByDescending(t => t.Id).ToListAsync());

        g.MapPost("/", async (TestimonialRequest req, AppDbContext db) =>
        {
            var t = new Testimonial();
            Apply(t, req);
            db.Testimonials.Add(t);
            await db.SaveChangesAsync();
            return Results.Ok(t);
        }).Validate<TestimonialRequest>();

        g.MapPut("/{id:int}", async (int id, TestimonialRequest req, AppDbContext db) =>
        {
            var t = await db.Testimonials.FindAsync(id);
            if (t is null) return Results.NotFound();
            Apply(t, req);
            await db.SaveChangesAsync();
            return Results.Ok(t);
        }).Validate<TestimonialRequest>();

        g.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
            await db.Testimonials.Where(t => t.Id == id).ExecuteDeleteAsync() == 0 ? Results.NotFound() : Results.NoContent());

        static void Apply(Testimonial t, TestimonialRequest r)
        {
            t.CustomerName = r.CustomerName.Trim();
            t.Location = r.Location.Trim();
            t.Quote = r.Quote.Trim();
            t.Rating = r.Rating;
            t.AvatarUrl = r.AvatarUrl.Trim();
            t.IsPublished = r.IsPublished;
        }
    }

    private static void MapConsultations(RouteGroupBuilder g)
    {
        g.MapGet("/", async (AppDbContext db, string? status) =>
        {
            var query = db.Consultations.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(k => k.Status == status);
            return await query.OrderByDescending(k => k.CreatedAt).Take(500).ToListAsync();
        });

        g.MapPut("/{id:int}/status", async (int id, ConsultationStatusRequest req, AppDbContext db) =>
        {
            if (!ConsultationStatus.All.Contains(req.Status))
                return Results.BadRequest(new { message = $"Status must be one of: {string.Join(", ", ConsultationStatus.All)}." });

            var updated = await db.Consultations.Where(k => k.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(k => k.Status, req.Status));
            return updated == 0 ? Results.NotFound() : Results.NoContent();
        }).Validate<ConsultationStatusRequest>();

        g.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
            await db.Consultations.Where(k => k.Id == id).ExecuteDeleteAsync() == 0 ? Results.NotFound() : Results.NoContent());
    }

    private static void MapMessages(RouteGroupBuilder g)
    {
        g.MapGet("/", async (AppDbContext db, string? site) =>
        {
            var query = db.ContactMessages.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(site)) query = query.Where(m => m.SiteKey == site);
            return await query.OrderByDescending(m => m.CreatedAt).Take(500).ToListAsync();
        });

        g.MapPut("/{id:int}/read", async (int id, AppDbContext db) =>
            await db.ContactMessages.Where(m => m.Id == id).ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true)) == 0
                ? Results.NotFound() : Results.NoContent());

        g.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
            await db.ContactMessages.Where(m => m.Id == id).ExecuteDeleteAsync() == 0 ? Results.NotFound() : Results.NoContent());
    }

    private static void MapCourses(RouteGroupBuilder admin)
    {
        var courses = admin.MapGroup("/courses");

        courses.MapGet("/", async (AppDbContext db, string? blog) =>
            await db.Courses.AsNoTracking().Where(c => blog == null || c.Blog == blog).OrderByDescending(c => c.UpdatedAt)
                .Select(c => new { c.Id, c.Blog, c.Title, c.Slug, c.Category, c.Level, c.IsPublished, c.IsFeatured, c.ThumbnailUrl, c.UpdatedAt, LessonCount = c.Lessons.Count })
                .ToListAsync());

        courses.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var course = await db.Courses.AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id, c.Blog, c.Title, c.Slug, c.Subtitle, c.Description, c.Category, c.Level, c.ThumbnailUrl, c.IsFeatured, c.IsPublished,
                    Lessons = c.Lessons.OrderBy(l => l.SortOrder)
                        .Select(l => new { l.Id, l.Title, l.Slug, l.DurationMinutes, l.SortOrder, l.IsPublished }).ToList()
                })
                .FirstOrDefaultAsync();
            return course is null ? Results.NotFound() : Results.Ok(course);
        });

        courses.MapPost("/", async (CourseRequest req, AppDbContext db) =>
        {
            var course = new Course();
            await ApplyAsync(course, req, db);
            db.Courses.Add(course);
            await db.SaveChangesAsync();
            return Results.Ok(new { course.Id, course.Slug });
        }).Validate<CourseRequest>();

        courses.MapPut("/{id:int}", async (int id, CourseRequest req, AppDbContext db) =>
        {
            var course = await db.Courses.FindAsync(id);
            if (course is null) return Results.NotFound();
            await ApplyAsync(course, req, db);
            await db.SaveChangesAsync();
            return Results.Ok(new { course.Id, course.Slug });
        }).Validate<CourseRequest>();

        courses.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var course = await db.Courses.FindAsync(id);
            if (course is null) return Results.NotFound();
            db.Courses.Remove(course);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        courses.MapPost("/{id:int}/lessons", async (int id, LessonRequest req, AppDbContext db) =>
        {
            if (!await db.Courses.AnyAsync(c => c.Id == id)) return Results.NotFound();
            var maxOrder = await db.Lessons.Where(l => l.CourseId == id).MaxAsync(l => (int?)l.SortOrder) ?? 0;
            var lesson = new Lesson { CourseId = id, SortOrder = req.SortOrder ?? maxOrder + 1 };
            await ApplyLessonAsync(lesson, req, db);
            db.Lessons.Add(lesson);
            await TouchCourseAsync(db, id);
            await db.SaveChangesAsync();
            return Results.Ok(new { lesson.Id, lesson.Slug });
        }).Validate<LessonRequest>();

        courses.MapPut("/{id:int}/lessons/order", async (int id, int[] lessonIds, AppDbContext db) =>
        {
            var lessons = await db.Lessons.Where(l => l.CourseId == id).ToListAsync();
            for (var i = 0; i < lessonIds.Length; i++)
            {
                var lesson = lessons.FirstOrDefault(l => l.Id == lessonIds[i]);
                if (lesson is not null) lesson.SortOrder = i + 1;
            }

            await TouchCourseAsync(db, id);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        var lessonsGroup = admin.MapGroup("/lessons");

        lessonsGroup.MapGet("/{id:int}", async (int id, AppDbContext db) =>
            await db.Lessons.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id) is { } lesson ? Results.Ok(lesson) : Results.NotFound());

        lessonsGroup.MapPut("/{id:int}", async (int id, LessonRequest req, AppDbContext db) =>
        {
            var lesson = await db.Lessons.FindAsync(id);
            if (lesson is null) return Results.NotFound();
            if (req.SortOrder is int order) lesson.SortOrder = order;
            await ApplyLessonAsync(lesson, req, db);
            await TouchCourseAsync(db, lesson.CourseId);
            await db.SaveChangesAsync();
            return Results.Ok(new { lesson.Id, lesson.Slug });
        }).Validate<LessonRequest>();

        lessonsGroup.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var lesson = await db.Lessons.FindAsync(id);
            if (lesson is null) return Results.NotFound();
            db.Lessons.Remove(lesson);
            await TouchCourseAsync(db, lesson.CourseId);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        static async Task ApplyAsync(Course c, CourseRequest r, AppDbContext db)
        {
            var baseSlug = Slug.Create(string.IsNullOrWhiteSpace(r.Slug) ? r.Title : r.Slug);
            c.Blog = r.Blog;
            c.Slug = await Slug.MakeUniqueAsync(baseSlug, s => db.Courses.AnyAsync(x => x.Blog == c.Blog && x.Slug == s && x.Id != c.Id));
            c.Title = r.Title.Trim();
            c.Subtitle = r.Subtitle.Trim();
            c.Description = r.Description.Trim();
            c.Category = r.Category.Trim();
            c.Level = r.Level.Trim();
            c.ThumbnailUrl = r.ThumbnailUrl.Trim();
            c.IsFeatured = r.IsFeatured;
            c.IsPublished = r.IsPublished;
            c.UpdatedAt = DateTime.UtcNow;
        }

        static async Task ApplyLessonAsync(Lesson l, LessonRequest r, AppDbContext db)
        {
            var baseSlug = Slug.Create(string.IsNullOrWhiteSpace(r.Slug) ? r.Title : r.Slug);
            l.Slug = await Slug.MakeUniqueAsync(baseSlug, s => db.Lessons.AnyAsync(x => x.CourseId == l.CourseId && x.Slug == s && x.Id != l.Id));
            l.Title = r.Title.Trim();
            l.Summary = r.Summary.Trim();
            l.Content = r.Content.Trim();
            l.VideoUrl = r.VideoUrl.Trim();
            l.DurationMinutes = r.DurationMinutes;
            l.IsPublished = r.IsPublished;
            l.UpdatedAt = DateTime.UtcNow;
        }

        static async Task TouchCourseAsync(AppDbContext db, int courseId)
        {
            var course = await db.Courses.FindAsync(courseId);
            if (course is not null) course.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static void MapPosts(RouteGroupBuilder g)
    {
        g.MapGet("/", async (AppDbContext db, string? blog) =>
            await db.Posts.AsNoTracking().Where(p => blog == null || p.Blog == blog).OrderByDescending(p => p.PublishedAt)
                .Select(p => new { p.Id, p.Blog, p.Title, p.Slug, p.Category, p.IsPublished, p.CoverImageUrl, p.PublishedAt, p.UpdatedAt })
                .ToListAsync());

        g.MapGet("/{id:int}", async (int id, AppDbContext db) =>
            await db.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id) is { } post ? Results.Ok(post) : Results.NotFound());

        g.MapPost("/", async (PostRequest req, AppDbContext db) =>
        {
            var post = new Post();
            await ApplyAsync(post, req, db);
            db.Posts.Add(post);
            await db.SaveChangesAsync();
            return Results.Ok(new { post.Id, post.Slug });
        }).Validate<PostRequest>();

        g.MapPut("/{id:int}", async (int id, PostRequest req, AppDbContext db) =>
        {
            var post = await db.Posts.FindAsync(id);
            if (post is null) return Results.NotFound();
            await ApplyAsync(post, req, db);
            await db.SaveChangesAsync();
            return Results.Ok(new { post.Id, post.Slug });
        }).Validate<PostRequest>();

        g.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
            await db.Posts.Where(p => p.Id == id).ExecuteDeleteAsync() == 0 ? Results.NotFound() : Results.NoContent());

        static async Task ApplyAsync(Post p, PostRequest r, AppDbContext db)
        {
            var baseSlug = Slug.Create(string.IsNullOrWhiteSpace(r.Slug) ? r.Title : r.Slug);
            p.Blog = r.Blog;
            p.Slug = await Slug.MakeUniqueAsync(baseSlug, s => db.Posts.AnyAsync(x => x.Blog == p.Blog && x.Slug == s && x.Id != p.Id));
            if (r.IsPublished && !p.IsPublished) p.PublishedAt = DateTime.UtcNow;
            p.Title = r.Title.Trim();
            p.Excerpt = r.Excerpt.Trim();
            p.Content = r.Content.Trim();
            p.Category = r.Category.Trim();
            p.Tags = r.Tags.Trim();
            p.CoverImageUrl = r.CoverImageUrl.Trim();
            p.Author = r.Author.Trim();
            p.IsPublished = r.IsPublished;
            p.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static readonly Dictionary<string, string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".png"] = "image/png", [".webp"] = "image/webp", [".gif"] = "image/gif"
    };

    private const long MaxUploadBytes = 5 * 1024 * 1024;

    private static void MapUploads(RouteGroupBuilder admin)
    {
        // Antiforgery tokens are replaced by the X-Requested-With check on this group.
        admin.MapPost("/uploads", async (IFormFile file, IWebHostEnvironment env) =>
        {
            var ext = Path.GetExtension(file.FileName);
            if (!AllowedImageTypes.ContainsKey(ext))
                return Results.BadRequest(new { message = "Only JPG, PNG, WEBP or GIF images are allowed." });
            if (file.Length is 0 or > MaxUploadBytes)
                return Results.BadRequest(new { message = "Images must be between 1 byte and 5 MB." });

            var relativeDir = Path.Combine(DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"));
            var dir = Path.Combine(UploadsRoot(env), relativeDir);
            Directory.CreateDirectory(dir);

            var name = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
            await using (var stream = File.Create(Path.Combine(dir, name)))
            {
                await file.CopyToAsync(stream);
            }

            return Results.Ok(new { url = $"/uploads/{relativeDir.Replace('\\', '/')}/{name}" });
        }).DisableAntiforgery();
    }

    public static string UploadsRoot(IWebHostEnvironment env) => Path.Combine(env.ContentRootPath, "App_Data", "uploads");
}
