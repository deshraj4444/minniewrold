using System.Security;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MinnieWorld.Web.Data;
using MinnieWorld.Web.Models;
using MinnieWorld.Web.Services.Cbse;
using MinnieWorld.Web.Services.Seo;

namespace MinnieWorld.Web.Endpoints;

/// <summary>robots.txt, sitemap.xml, the 301s for moved pages, and the public solutions of the free JEE chapter tests.</summary>
public static class SeoEndpoints
{
    /// <summary>Permanently redirects old and non-canonical URLs (e.g. /gk/jee → /jee). Runs before static files and routing.</summary>
    public static IApplicationBuilder UseSeoRedirects(this IApplicationBuilder app) => app.Use(async (ctx, next) =>
    {
        var path = ctx.Request.Path.Value ?? "/";
        if ((HttpMethods.IsGet(ctx.Request.Method) || HttpMethods.IsHead(ctx.Request.Method))
            && (JeePages.RedirectFor(path) ?? CbseRedirectFor(path)) is { } target)
        {
            ctx.Response.StatusCode = StatusCodes.Status301MovedPermanently;
            ctx.Response.Headers.Location = target + ctx.Request.QueryString;
            return;
        }

        await next();
    });

    /// <summary>/CBSE/Class-6/ → /cbse/class-6: CBSE URLs are lowercase with no trailing slash. With one class, /cbse goes to it.</summary>
    public static string? CbseRedirectFor(string path)
    {
        if (!path.StartsWith("/cbse", StringComparison.OrdinalIgnoreCase) || (path.Length > 5 && path[5] != '/')) return null;
        var canonical = path.Length > 1 ? path.TrimEnd('/').ToLowerInvariant() : path;
        if (canonical == CbseCatalog.HubPath) canonical = CbseCatalog.EntryPath;
        return canonical != path ? canonical : null;
    }

    public static void MapSeoEndpoints(this WebApplication app)
    {
        app.MapGet("/robots.txt", (HttpContext http, IConfiguration config) =>
        {
            var origin = Origin(http, config);
            var text = new StringBuilder()
                .AppendLine("User-agent: *")
                .AppendLine("Allow: /")
                // Private pages only. /api stays open: crawlers that run JavaScript need it to render the pages.
                .AppendLine("Disallow: /admin")
                .AppendLine("Disallow: /gk/attempt/")
                .AppendLine("Disallow: /gk/my-tests")
                .AppendLine("Disallow: /gk/login")
                .AppendLine("Disallow: /gk/register")
                .AppendLine()
                .AppendLine($"Sitemap: {origin}/sitemap.xml")
                .ToString();
            return Results.Text(text, "text/plain; charset=utf-8");
        });

        app.MapGet("/sitemap.xml", async (HttpContext http, IConfiguration config, AppDbContext db, CbseCatalog cbse, CancellationToken ct) =>
        {
            var origin = Origin(http, config);
            var testSlugs = JeePages.All.Where(p => p.Kind == "test").Select(p => p.Slug).ToList();
            var noteSlugs = JeePages.All.Where(p => p.Kind == "notes").Select(p => p.Slug).ToList();
            var tests = await db.Exams.AsNoTracking().Where(e => e.IsPublished && testSlugs.Contains(e.Slug))
                .ToDictionaryAsync(e => e.Slug, e => e.UpdatedAt, ct);
            var notes = await db.Posts.AsNoTracking().Where(p => p.Blog == BlogKeys.Gk && p.IsPublished && noteSlugs.Contains(p.Slug))
                .ToDictionaryAsync(p => p.Slug, p => p.UpdatedAt, ct);

            var pages = JeePages.All
                .Select(p => (p.Path, Modified: (p.Kind == "test" ? tests : notes).TryGetValue(p.Slug, out var d) ? d : (DateTime?)null))
                .Where(p => p.Modified is not null)
                .ToList();

            var urls = new List<(string Path, DateTime? Modified)>
            {
                ("/", null),
                ("/gk", null),
                ("/gk/tests", null),
                (JeePages.HubPath, pages.Count > 0 ? pages.Max(p => p.Modified) : null)
            };
            urls.AddRange(pages);

            // CBSE: each class and subject that has chapters, then the chapters. Empty pages are noindex, so they're left out.
            var chapters = cbse.All();
            DateTime? Latest(IEnumerable<Services.Cbse.CbseChapter> list) =>
                list.Select(Services.Cbse.CbseCatalog.Modified).Where(d => d is not null).Select(d => (DateTime?)d!.Value.ToDateTime(TimeOnly.MinValue)).DefaultIfEmpty(null).Max();
            if (chapters.Count > 0 && Services.Cbse.CbseCatalog.EntryPath == Services.Cbse.CbseCatalog.HubPath) urls.Add((Services.Cbse.CbseCatalog.HubPath, Latest(chapters)));
            foreach (var n in Services.Cbse.CbseCatalog.Classes.Where(n => chapters.Any(c => c.Class == n)))
            {
                urls.Add((Services.Cbse.CbseCatalog.ClassPath(n), Latest(chapters.Where(c => c.Class == n))));
                foreach (var s in Services.Cbse.CbseCatalog.Subjects)
                {
                    var list = cbse.Chapters(n, s.Key);
                    if (list.Count == 0) continue;
                    urls.Add((Services.Cbse.CbseCatalog.SubjectPath(n, s.Key), Latest(list)));
                    urls.AddRange(list.Select(c => (Services.Cbse.CbseCatalog.ChapterPath(c), Latest([c]))));
                }
            }

            var xml = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n");
            foreach (var (path, modified) in urls)
            {
                xml.Append($"  <url><loc>{SecurityElement.Escape(origin + path)}</loc>");
                if (modified is { } m)
                {
                    xml.Append($"<lastmod>{m:yyyy-MM-dd}</lastmod>");
                }

                xml.Append("</url>\n");
            }

            xml.Append("</urlset>\n");
            return Results.Text(xml.ToString(), "application/xml; charset=utf-8");
        });

        // Every question with its answer and solution, for the free JEE chapter tests only (shown on their public page).
        app.MapGet("/api/jee/solutions/{slug}", async (string slug, AppDbContext db, CancellationToken ct) =>
        {
            if (!JeePages.HasPublicSolutions(slug))
            {
                return Results.NotFound();
            }

            var exam = await db.Exams.AsNoTracking().Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.Slug == slug && e.IsPublished && e.Price == 0, ct);
            if (exam is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(exam.Questions.OrderBy(q => q.SortOrder).Select(q => new
            {
                q.Id, q.Section, q.Text, q.TextHi, q.Options,
                OptionsHi = q.OptionsHi.All(o => !string.IsNullOrWhiteSpace(o)) ? q.OptionsHi : null,
                q.Answer, q.Explanation, q.ExplanationHi, q.Marks,
                Figure = Blank(q.Figure), Passage = Blank(q.Passage), PassageHi = Blank(q.PassageHi)
            }));
        });
    }

    private static string? Blank(string s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private static string Origin(HttpContext http, IConfiguration config)
    {
        var configured = config["Site:PublicUrl"]?.TrimEnd('/');
        return string.IsNullOrWhiteSpace(configured) ? $"{http.Request.Scheme}://{http.Request.Host}" : configured;
    }
}
