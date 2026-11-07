using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MinnieWorld.Web.Data;
using MinnieWorld.Web.Models;

namespace MinnieWorld.Web.Services;

/// <summary>
/// Serves the React index.html for every non-API route, with page-specific title and Open Graph tags
/// filled in server-side. Facebook, WhatsApp, LinkedIn and X don't run JavaScript, so this is what
/// makes a shared course link show its own title, description and thumbnail.
/// </summary>
public partial class SpaHost(IWebHostEnvironment env, IConfiguration config, Cbse.CbseCatalog cbse)
{
    private const string HeadPlaceholder = "<!--app-head-->";
    private const string RootDiv = "<div id=\"root\"></div>";
    private string? _template;

    /// <param name="Canonical">The canonical path when it differs from the requested one.</param>
    /// <param name="JsonLd">Structured data objects, each written as its own application/ld+json script.</param>
    /// <param name="Body">Server-rendered page content placed after the React root, so crawlers see it without running JavaScript.</param>
    private record PageMeta(string Title, string Description, string? Image = null, string Type = "website", bool NoIndex = false,
        string? Canonical = null, IReadOnlyList<object>? JsonLd = null, string? Body = null, int Status = StatusCodes.Status200OK);

    public async Task ServeAsync(HttpContext context, AppDbContext db)
    {
        var template = await LoadTemplateAsync();
        if (template is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("Client app not built. Run `npm run build` in /client, or use `npm run dev` during development.");
            return;
        }

        var path = context.Request.Path.Value ?? "/";
        var meta = await ResolveAsync(path, db, context, context.RequestAborted);
        var html = template.Replace(HeadPlaceholder, BuildHead(meta, AbsoluteUrl(context, meta.Canonical ?? path), context));
        if (meta.Body is not null)
        {
            html = html.Replace(RootDiv, RootDiv + meta.Body);
        }

        context.Response.StatusCode = meta.Status;
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.Headers.CacheControl = "no-cache";
        await context.Response.WriteAsync(html);
    }

    private async Task<string?> LoadTemplateAsync()
    {
        if (_template is not null && !env.IsDevelopment())
        {
            return _template;
        }

        var file = env.WebRootFileProvider.GetFileInfo("index.html");
        if (!file.Exists)
        {
            return null;
        }

        await using var stream = file.CreateReadStream();
        using var reader = new StreamReader(stream);
        return _template = await reader.ReadToEndAsync();
    }

    private static readonly Dictionary<string, (string Name, string Tagline)> Blogs = new()
    {
        [BlogKeys.Dharma] = ("Dharma Path", "Scriptures, mantras, festivals and temples: Sanatan wisdom in simple words."),
        [BlogKeys.Gk] = ("Curious Minds", "General knowledge made fun: science, India, the world, and exam-ready courses.")
    };

    private static readonly Dictionary<string, PageMeta> AstroPages = new()
    {
        ["kundali"] = new("Free Kundali Online: Janam Kundli with Dasha | AstroAura", "Make your free Janam Kundali: Lagna chart, planet positions, nakshatra, Vimshottari dasha and Manglik check."),
        ["matching"] = new("Kundli Milan: 36 Guna Matching for Marriage | AstroAura", "Free Ashtakoota kundli matching with all 8 kootas, Nadi and Bhakoot dosha, and Manglik compatibility."),
        ["panchang"] = new("Today's Panchang: Tithi, Nakshatra, Rahu Kaal | AstroAura", "Daily Panchang for your city: tithi, nakshatra, yoga, karana, sunrise, sunset, Rahu Kaal and Abhijit Muhurat."),
        ["numerology"] = new("Numerology Calculator: Moolank & Bhagyank | AstroAura", "Find your Moolank, Bhagyank and name number with lucky numbers, colours and days."),
        ["horoscope"] = new("Daily, Weekly & Monthly Horoscope | AstroAura", "Free horoscope for all 12 zodiac signs, updated every day."),
        ["consultation"] = new("Talk to an Astrologer | AstroAura", "Book a personal consultation for career, marriage, health and kundali reading."),
        ["contact"] = new("Contact | AstroAura", "Reach the AstroAura astrology desk.")
    };

    private async Task<PageMeta> ResolveAsync(string path, AppDbContext db, HttpContext context, CancellationToken ct)
    {
        var parts = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var section = parts.ElementAtOrDefault(0)?.ToLowerInvariant() ?? "";

        if (section == "jee")
        {
            return await ResolveJeeAsync(path, db, context, ct);
        }

        if (section == "cbse")
        {
            return ResolveCbse(parts, context);
        }

        if (section == "admin")
        {
            return new PageMeta("Admin | MinnieWorld", "Site administration.", NoIndex: true);
        }

        if (section == "astro")
        {
            if (parts.Length >= 3 && parts[1] == "horoscope" && HoroscopeService.IsValidSign(parts[2]))
            {
                var sign = char.ToUpperInvariant(parts[2][0]) + parts[2][1..].ToLowerInvariant();
                return new PageMeta($"{sign} Horoscope Today | AstroAura", $"Today's {sign} horoscope: love, career, money and health predictions from AstroAura.");
            }

            return AstroPages.TryGetValue(parts.ElementAtOrDefault(1) ?? "", out var page)
                ? page
                : new PageMeta("AstroAura | Free Kundali, Horoscope, Panchang & Kundli Milan", "Free Janam Kundali, kundli matching, daily horoscope, Panchang and numerology, all based on Vedic astrology.");
        }

        if (Blogs.TryGetValue(section, out var blog))
        {
            if (parts.Length >= 3 && parts[1] == "courses")
            {
                var course = await db.Courses.AsNoTracking()
                    .Where(c => c.Blog == section && c.Slug == parts[2] && c.IsPublished)
                    .Select(c => new { c.Id, c.Title, c.Subtitle, c.ThumbnailUrl })
                    .FirstOrDefaultAsync(ct);

                if (course is not null && parts.Length >= 4)
                {
                    var lesson = await db.Lessons.AsNoTracking()
                        .Where(l => l.CourseId == course.Id && l.Slug == parts[3] && l.IsPublished)
                        .Select(l => new { l.Title, l.Summary })
                        .FirstOrDefaultAsync(ct);
                    if (lesson is not null)
                    {
                        return new PageMeta($"{lesson.Title} · {course.Title}", lesson.Summary, course.ThumbnailUrl, "article");
                    }
                }

                if (course is not null)
                {
                    return new PageMeta($"{course.Title} | {blog.Name}", course.Subtitle, course.ThumbnailUrl, "article");
                }
            }

            // Link previews on Facebook/WhatsApp for the mock tests (the crawlers don't run the React app).
            if (section == "gk" && parts.Length >= 2 && parts[1] == "tests")
            {
                if (parts.Length >= 3)
                {
                    var exam = await db.Exams.AsNoTracking()
                        .Where(e => e.Slug == parts[2] && e.IsPublished)
                        .Select(e => new { e.Title, e.Description, e.Price, Count = e.Questions.Count })
                        .FirstOrDefaultAsync(ct);
                    if (exam is not null)
                    {
                        var price = exam.Price > 0 ? $"₹{exam.Price}" : "Free";
                        return new PageMeta($"{exam.Title} | Online Mock Test", $"{exam.Count} questions in English + हिंदी with instant result, rank and answer key. {price}. {exam.Description}",
                            TestThumbnail(parts[2]));
                    }
                }

                return new PageMeta("Free Mock Tests: Class 6 Admission (JNV, RMS, Sainik), HP TET & CTET | Curious Minds",
                    "Navodaya, Military School and Sainik School Class 6 practice papers, plus HP TET and CTET mock tests and previous year papers, in English and Hindi, with instant results and certificates.",
                    TestThumbnail("index"));
            }

            if (section == "gk" && parts.Length >= 2 && parts[1] == "class-6")
            {
                return new PageMeta("Free Class 6 Admission Tests: Navodaya, Military School & Sainik School | Curious Minds",
                    "Free JNVST, RMS CET and Sainik School AISSEE Class 6 practice papers: past 5 years' patterns and 5 expected papers each, in English + हिंदी, with certificates and a quiz game.",
                    TestThumbnail("index"));
            }

            // An exam's page or quiz game (/gk/exams/jnvst-class-6, /gk/play/jnvst-class-6): its tests' slugs start with the key.
            if (section == "gk" && parts.Length >= 3 && parts[1] is "exams" or "play")
            {
                var key = parts[2].ToLowerInvariant();
                var category = await db.Exams.AsNoTracking()
                    .Where(e => e.IsPublished && e.Slug.StartsWith(key + "-"))
                    .Select(e => e.Category)
                    .FirstOrDefaultAsync(ct);
                if (category is not null)
                {
                    return parts[1] == "play"
                        ? new PageMeta($"{category} Quiz Game | Curious Minds", $"Play a quick 10-question {category} quiz with lives, streaks and stars. Free, in English + हिंदी.", TestThumbnail("index"))
                        : new PageMeta($"{category}: Free Practice Papers | Curious Minds", $"Free {category} practice papers in English + हिंदी with instant results, answer keys and certificates.", TestThumbnail("index"));
                }
            }

            if (section == "gk" && parts.Length >= 3 && parts[1] == "certificate" && Services.Exams.Certificates.AttemptId(parts[2]) is { } attemptId)
            {
                var a = await db.ExamAttempts.AsNoTracking()
                    .Where(x => x.Id == attemptId && x.SubmittedAt != null)
                    .Select(x => new { x.Id, x.StudentId, x.StartedAt, x.Score, x.MaxScore, Name = x.Student!.FullName, x.Exam!.Title })
                    .FirstOrDefaultAsync(ct);
                if (a is not null && Services.Exams.Certificates.Earned(a.Score, a.MaxScore)
                    && string.Equals(Services.Exams.Certificates.Code(a.Id, a.StudentId, a.StartedAt), parts[2], StringComparison.OrdinalIgnoreCase))
                {
                    var level = Services.Exams.Certificates.Level(a.Score, a.MaxScore);
                    return new PageMeta($"🏅 {a.Name} earned a {level} certificate | Curious Minds",
                        $"{a.Name} passed {a.Title} with {a.Score:0.##}/{a.MaxScore}. Practise free on Curious Minds.", TestThumbnail("index"), NoIndex: true);
                }
            }

            if (parts.Length >= 3 && parts[1] == "posts")
            {
                var post = await db.Posts.AsNoTracking()
                    .Where(p => p.Blog == section && p.Slug == parts[2] && p.IsPublished)
                    .Select(p => new { p.Title, p.Excerpt, p.CoverImageUrl })
                    .FirstOrDefaultAsync(ct);
                if (post is not null)
                {
                    return new PageMeta($"{post.Title} | {blog.Name}", post.Excerpt, post.CoverImageUrl, "article");
                }
            }

            return new PageMeta(blog.Name, blog.Tagline);
        }

        return new PageMeta("MinnieWorld", "AstroAura astrology, Dharma Path and Curious Minds, all in one place.");
    }

    /// <summary>The share thumbnail made by tools/make-og-images.mjs, or the general tests one for a test added later in admin.</summary>
    private string TestThumbnail(string slug)
    {
        var file = Path.Combine(env.WebRootPath ?? "", "og", "tests", $"{slug}.png");
        return File.Exists(file) && !slug.Contains("..") ? $"/og/tests/{slug}.png" : "/og/tests/index.png";
    }

    private string BuildHead(PageMeta meta, string url, HttpContext context)
    {
        static string E(string s) => WebUtility.HtmlEncode(s);
        var sb = new StringBuilder();
        sb.Append($"<title>{E(meta.Title)}</title>");
        sb.Append($"<meta name=\"description\" content=\"{E(meta.Description)}\">");
        sb.Append($"<link rel=\"canonical\" href=\"{E(url)}\">");
        sb.Append($"<meta property=\"og:title\" content=\"{E(meta.Title)}\">");
        sb.Append($"<meta property=\"og:description\" content=\"{E(meta.Description)}\">");
        sb.Append($"<meta property=\"og:type\" content=\"{meta.Type}\">");
        sb.Append($"<meta property=\"og:url\" content=\"{E(url)}\">");
        sb.Append($"<meta property=\"og:site_name\" content=\"{E(SiteName(meta.Canonical ?? context.Request.Path.Value ?? "/"))}\">");
        sb.Append("<meta name=\"twitter:card\" content=\"summary_large_image\">");
        sb.Append($"<meta name=\"twitter:title\" content=\"{E(meta.Title)}\">");
        sb.Append($"<meta name=\"twitter:description\" content=\"{E(meta.Description)}\">");
        if (!string.IsNullOrWhiteSpace(meta.Image))
        {
            var image = meta.Image.StartsWith('/') ? AbsoluteUrl(context, meta.Image) : meta.Image;
            sb.Append($"<meta property=\"og:image\" content=\"{E(image)}\">");
            sb.Append($"<meta name=\"twitter:image\" content=\"{E(image)}\">");
            if (image.Contains("/og/tests/"))
            {
                // Lets Facebook show the large preview on the first share, before it has fetched the image.
                sb.Append("<meta property=\"og:image:width\" content=\"1200\"><meta property=\"og:image:height\" content=\"630\">");
                sb.Append($"<meta property=\"og:image:alt\" content=\"{E(meta.Title)}\">");
            }
        }

        if (meta.NoIndex)
        {
            sb.Append("<meta name=\"robots\" content=\"noindex\">");
        }

        foreach (var item in meta.JsonLd ?? [])
        {
            // The default encoder escapes <, > and &, so the JSON can't close the script tag early.
            sb.Append($"<script type=\"application/ld+json\">{System.Text.Json.JsonSerializer.Serialize(item)}</script>");
        }

        return sb.ToString();
    }

    private static string SiteName(string path) => path.TrimStart('/').Split('/')[0].ToLowerInvariant() switch
    {
        "astro" => "AstroAura",
        "dharma" => "Dharma Path",
        "gk" or "jee" or "cbse" => "Curious Minds",
        _ => "MinnieWorld"
    };

    private string AbsoluteUrl(HttpContext context, string path)
    {
        var configured = config["Site:PublicUrl"]?.TrimEnd('/');
        var origin = string.IsNullOrWhiteSpace(configured) ? $"{context.Request.Scheme}://{context.Request.Host}" : configured;
        return origin + path;
    }
}
