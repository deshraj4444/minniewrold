using Microsoft.EntityFrameworkCore;
using MinnieWorld.Web.Contracts;
using MinnieWorld.Web.Data;
using MinnieWorld.Web.Infrastructure;
using MinnieWorld.Web.Models;
using MinnieWorld.Web.Services;
using MinnieWorld.Web.Services.Astrology;

namespace MinnieWorld.Web.Endpoints;

public static partial class PublicEndpoints
{
    public static void MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

        // Public site links. The Google review link is only passed on if it really points at Google.
        api.MapGet("/site-info", async (AppDbContext db) =>
        {
            var s = await AdminExamEndpoints.SettingsAsync(db);
            var review = s.GetValueOrDefault(SettingKeys.GoogleReviewUrl, "").Trim();
            return Results.Ok(new { GoogleReviewUrl = IsGoogleLink(review) ? review : null });
        });

        // Site-wide visitor counter shown in the footers. Each browser counts once per day (India time);
        // bots and link-preview crawlers aren't counted. The shown total starts at VisitorCounter:Start (2001).
        api.MapGet("/visits", async (AppDbContext db, IConfiguration config) => Results.Ok(new { total = await VisitorTotalAsync(db, config) }));

        api.MapPost("/visits", async (AppDbContext db, IConfiguration config, HttpContext http) =>
        {
            const string cookie = "mw_visit";
            var india = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, OperatingSystem.IsWindows() ? "India Standard Time" : "Asia/Kolkata");
            var today = india.ToString("yyyy-MM-dd");
            var agent = http.Request.Headers.UserAgent.ToString();
            var isBot = agent.Length == 0 || BotAgent().IsMatch(agent);

            if (!isBot && http.Request.Cookies[cookie] != today)
            {
                if (await db.Counters.Where(c => c.Key == SiteCounter.Visitors).ExecuteUpdateAsync(c => c.SetProperty(x => x.Value, x => x.Value + 1)) == 0)
                {
                    db.Counters.Add(new SiteCounter { Key = SiteCounter.Visitors, Value = 1 });
                    try { await db.SaveChangesAsync(); }
                    catch (DbUpdateException)
                    {
                        // Another request created the row first.
                        db.ChangeTracker.Clear();
                        await db.Counters.Where(c => c.Key == SiteCounter.Visitors).ExecuteUpdateAsync(c => c.SetProperty(x => x.Value, x => x.Value + 1));
                    }
                }

                http.Response.Cookies.Append(cookie, today, new CookieOptions
                {
                    HttpOnly = true, Secure = http.Request.IsHttps, SameSite = SameSiteMode.Lax, IsEssential = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(2)
                });
            }

            return Results.Ok(new { total = await VisitorTotalAsync(db, config) });
        }).RequireRateLimiting(RateLimits.Visits);

        api.MapGet("/sites", async (AppDbContext db) =>
            await db.Sites.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.SortOrder).ThenBy(s => s.Name).ToListAsync());

        api.MapPost("/contact", async (ContactRequest req, AppDbContext db) =>
        {
            db.ContactMessages.Add(new ContactMessage
            {
                SiteKey = req.SiteKey,
                FullName = req.FullName.Trim(),
                Email = req.Email.Trim(),
                Phone = req.Phone.Trim(),
                Subject = req.Subject.Trim(),
                Message = req.Message.Trim()
            });
            await db.SaveChangesAsync();
            return Results.Ok(new { message = "Thank you, we'll get back to you soon." });
        }).Validate<ContactRequest>().RequireRateLimiting(RateLimits.Forms);

        MapAstro(api.MapGroup("/astro"));
        MapBlogs(api.MapGroup("/blogs/{blog}").AddEndpointFilter(async (ctx, next) =>
            BlogKeys.IsValid(ctx.HttpContext.GetRouteValue("blog") as string) ? await next(ctx) : Results.NotFound()));
    }

    private static void MapAstro(RouteGroupBuilder astro)
    {
        astro.MapGet("/horoscopes/{sign}", async (string sign, HoroscopeService horoscopes, CancellationToken ct, string period = "daily") =>
        {
            if (!HoroscopeService.IsValidSign(sign)) return Results.BadRequest(new { message = "Unknown zodiac sign." });
            if (!HoroscopeService.Periods.Contains(period)) return Results.BadRequest(new { message = "Period must be daily, weekly or monthly." });
            return Results.Ok(await horoscopes.GetAsync(sign, period, ct));
        });

        astro.MapGet("/places", async (string q, PlaceService places, CancellationToken ct) => await places.SearchAsync(q, ct));

        astro.MapGet("/panchang", (DateOnly? date, double? lat, double? lon, string? tz, string? place) =>
        {
            // Defaults to New Delhi when no place is given.
            var timeZone = string.IsNullOrWhiteSpace(tz) ? "Asia/Kolkata" : tz;
            if (!PlaceService.IsKnownTimeZone(timeZone)) return Results.BadRequest(new { message = "Unknown time zone." });
            if (lat is < -66.5 or > 66.5) return Results.BadRequest(new { message = "Panchang is not supported above the polar circles." });

            var day = date ?? DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(timeZone)));
            return Results.Ok(PanchangCalculator.Calculate(day, lat ?? 28.6139, lon ?? 77.2090, timeZone, string.IsNullOrWhiteSpace(place) ? "New Delhi, India" : place));
        });

        astro.MapPost("/kundali", (BirthInput input) =>
            TryBirth(input, out var birth, out var error) ? Results.Ok(KundaliCalculator.Calculate(birth!, DateTime.UtcNow)) : error!)
            .Validate<BirthInput>();

        astro.MapPost("/matching", (MatchRequest req) =>
        {
            if (!TryBirth(req.Boy!, out var boy, out var error) || !TryBirth(req.Girl!, out var girl, out error)) return error!;
            var now = DateTime.UtcNow;
            return Results.Ok(Matching.Match(KundaliCalculator.Calculate(boy!, now), KundaliCalculator.Calculate(girl!, now)));
        }).Validate<MatchRequest>();

        astro.MapGet("/numerology", (string name, DateOnly dob) =>
            string.IsNullOrWhiteSpace(name) || name.Length > 80
                ? Results.BadRequest(new { message = "Please enter your name." })
                : Results.Ok(Numerology.Calculate(name, dob)));

        astro.MapGet("/contact-info", async (AppDbContext db) =>
        {
            var s = await AdminExamEndpoints.SettingsAsync(db);
            return new
            {
                WhatsAppNumber = new string(s[SettingKeys.WhatsAppNumber].Where(char.IsDigit).ToArray()),
                Greeting = s[SettingKeys.WhatsAppGreeting],
                AstrologerName = s[SettingKeys.AstrologerName]
            };
        });

        astro.MapPost("/varshphal", (VarshphalRequest req) =>
        {
            if (!TryBirth(req.Birth!, out var birth, out var error)) return error!;
            if (req.Year < birth!.Date.Year) return Results.BadRequest(new { message = "The Varshphal year can't be before the year of birth." });
            return Results.Ok(VarshphalCalculator.Calculate(birth, req.Year));
        }).Validate<VarshphalRequest>();

        astro.MapGet("/testimonials", async (AppDbContext db) =>
            await db.Testimonials.AsNoTracking().Where(t => t.IsPublished).OrderByDescending(t => t.Rating).ThenByDescending(t => t.Id).ToListAsync());

        astro.MapPost("/consultations", async (ConsultationRequestDto req, AppDbContext db) =>
        {
            db.Consultations.Add(new ConsultationRequest
            {
                FullName = req.FullName.Trim(),
                Email = req.Email.Trim(),
                Phone = req.Phone.Trim(),
                BirthDate = req.BirthDate!.Value,
                BirthTime = req.BirthTime!.Value,
                BirthPlace = req.BirthPlace.Trim(),
                ServiceType = req.ServiceType.Trim(),
                Notes = req.Notes.Trim()
            });
            await db.SaveChangesAsync();
            return Results.Ok(new { message = "Your request is in. Our astrologer will contact you within 24 hours." });
        }).Validate<ConsultationRequestDto>().RequireRateLimiting(RateLimits.Forms);
    }

    private static bool TryBirth(BirthInput input, out BirthDetails? birth, out IResult? error)
    {
        birth = null;
        error = null;
        if (!PlaceService.IsKnownTimeZone(input.TimeZone))
        {
            error = Results.BadRequest(new { message = "Unknown time zone. Please pick the birth place from the list." });
            return false;
        }

        if (input.Date!.Value.Year is < 1800 or > 2100)
        {
            error = Results.BadRequest(new { message = "Birth year must be between 1800 and 2100." });
            return false;
        }

        birth = new BirthDetails(input.Name.Trim(), input.Date.Value, input.Time!.Value, input.Latitude, input.Longitude, input.TimeZone, input.Place.Trim());
        return true;
    }

    private static void MapBlogs(RouteGroupBuilder blog)
    {
        blog.MapGet("/categories", async (string blog, AppDbContext db) =>
        {
            var courseCats = await db.Courses.Where(c => c.Blog == blog && c.IsPublished).Select(c => c.Category).Distinct().ToListAsync();
            var postCats = await db.Posts.Where(p => p.Blog == blog && p.IsPublished).Select(p => p.Category).Distinct().ToListAsync();
            return new { courses = courseCats.Order(), posts = postCats.Order() };
        });

        blog.MapGet("/courses", async (string blog, AppDbContext db, string? category, string? q, bool? featured) =>
        {
            var query = db.Courses.AsNoTracking().Where(c => c.Blog == blog && c.IsPublished);
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(c => c.Category == category);
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.Title.Contains(q) || c.Subtitle.Contains(q));
            if (featured == true) query = query.Where(c => c.IsFeatured);

            return await query
                .OrderByDescending(c => c.IsFeatured).ThenByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id, c.Title, c.Slug, c.Subtitle, c.Category, c.Level, c.ThumbnailUrl, c.IsFeatured, c.UpdatedAt,
                    LessonCount = c.Lessons.Count(l => l.IsPublished),
                    TotalMinutes = c.Lessons.Where(l => l.IsPublished).Sum(l => l.DurationMinutes)
                })
                .ToListAsync();
        });

        blog.MapGet("/courses/{slug}", async (string blog, string slug, AppDbContext db) =>
        {
            var course = await db.Courses.AsNoTracking()
                .Where(c => c.Blog == blog && c.Slug == slug && c.IsPublished)
                .Select(c => new
                {
                    c.Id, c.Title, c.Slug, c.Subtitle, c.Description, c.Category, c.Level, c.ThumbnailUrl, c.UpdatedAt,
                    Lessons = c.Lessons.Where(l => l.IsPublished).OrderBy(l => l.SortOrder)
                        .Select(l => new { l.Id, l.Title, l.Slug, l.Summary, l.DurationMinutes, HasVideo = l.VideoUrl != "" })
                        .ToList()
                })
                .FirstOrDefaultAsync();
            return course is null ? Results.NotFound() : Results.Ok(course);
        });

        blog.MapGet("/courses/{slug}/lessons/{lessonSlug}", async (string blog, string slug, string lessonSlug, AppDbContext db) =>
        {
            var course = await db.Courses.AsNoTracking()
                .Where(c => c.Blog == blog && c.Slug == slug && c.IsPublished)
                .Select(c => new
                {
                    c.Id, c.Title, c.Slug, c.ThumbnailUrl,
                    Lessons = c.Lessons.Where(l => l.IsPublished).OrderBy(l => l.SortOrder)
                        .Select(l => new { l.Title, l.Slug, l.DurationMinutes }).ToList()
                })
                .FirstOrDefaultAsync();
            if (course is null) return Results.NotFound();

            var index = course.Lessons.FindIndex(l => l.Slug == lessonSlug);
            if (index < 0) return Results.NotFound();

            var lesson = await db.Lessons.AsNoTracking()
                .Where(l => l.CourseId == course.Id && l.Slug == lessonSlug)
                .Select(l => new { l.Title, l.Slug, l.Summary, l.Content, l.VideoUrl, l.DurationMinutes, l.UpdatedAt })
                .FirstAsync();

            return Results.Ok(new
            {
                Course = new { course.Title, course.Slug, course.ThumbnailUrl, course.Lessons },
                Lesson = lesson,
                Position = index + 1,
                Previous = index > 0 ? course.Lessons[index - 1] : null,
                Next = index < course.Lessons.Count - 1 ? course.Lessons[index + 1] : null
            });
        });

        blog.MapGet("/posts", async (string blog, AppDbContext db, string? category, string? q, int page = 1, int pageSize = 9) =>
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = db.Posts.AsNoTracking().Where(p => p.Blog == blog && p.IsPublished);
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(p => p.Category == category);
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Title.Contains(q) || p.Excerpt.Contains(q) || p.Tags.Contains(q));

            var total = await query.CountAsync();
            var items = await query.OrderByDescending(p => p.PublishedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new { p.Id, p.Title, p.Slug, p.Excerpt, p.Category, p.Tags, p.CoverImageUrl, p.Author, p.PublishedAt, ReadMinutes = (p.Content.Length / 1100) + 1 })
                .ToListAsync();

            return new { items, total, page, pageSize };
        });

        blog.MapGet("/posts/{slug}", async (string blog, string slug, AppDbContext db) =>
        {
            var post = await db.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Blog == blog && p.Slug == slug && p.IsPublished);
            if (post is null) return Results.NotFound();

            var related = await db.Posts.AsNoTracking()
                .Where(p => p.Blog == blog && p.IsPublished && p.Id != post.Id)
                .OrderByDescending(p => p.Category == post.Category).ThenByDescending(p => p.PublishedAt).Take(3)
                .Select(p => new { p.Title, p.Slug, p.Excerpt, p.CoverImageUrl, p.Category, p.PublishedAt })
                .ToListAsync();

            return Results.Ok(new { post, readMinutes = (post.Content.Length / 1100) + 1, related });
        });
    }

    private static async Task<long> VisitorTotalAsync(AppDbContext db, IConfiguration config) =>
        (config.GetValue<long?>("VisitorCounter:Start") ?? 2001)
        + (await db.Counters.AsNoTracking().Where(c => c.Key == SiteCounter.Visitors).Select(c => (long?)c.Value).FirstOrDefaultAsync() ?? 0);

    /// <summary>True for an https link to Google (google.*, g.page, maps.app.goo.gl, goo.gl), so the button can't point anywhere else.</summary>
    public static bool IsGoogleLink(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
        && (uri.Host is "g.page" or "maps.app.goo.gl" or "goo.gl"
            || System.Text.RegularExpressions.Regex.IsMatch(uri.Host, @"^([a-z0-9-]+\.)*google\.[a-z]{2,3}(\.[a-z]{2})?$", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

    [System.Text.RegularExpressions.GeneratedRegex(@"bot|crawl|spider|slurp|facebookexternalhit|whatsapp|preview|headless|curl|wget|python|httpclient", System.Text.RegularExpressions.RegexOptions.IgnoreCase)]
    private static partial System.Text.RegularExpressions.Regex BotAgent();
}
