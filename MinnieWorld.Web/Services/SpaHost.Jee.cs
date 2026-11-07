using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Microsoft.EntityFrameworkCore;
using MinnieWorld.Web.Data;
using MinnieWorld.Web.Models;
using MinnieWorld.Web.Services.Seo;

namespace MinnieWorld.Web.Services;

/// <summary>
/// The IIT JEE pages (/jee, /jee/physics/kinematics-test, /jee/physics/kinematics-notes). Besides the head tags,
/// these pages are rendered on the server: every question, option, solution and the full notes are in the first
/// HTML response, so search engines index them without running JavaScript. The React page replaces this markup
/// once it has loaded (see dropPrerender in client/src/lib/prerender.js), so the two must show the same content.
/// </summary>
public partial class SpaHost
{
    private const string SiteBrand = "Curious Minds";
    private static readonly string[] Letters = ["A", "B", "C", "D"];

    // Raw HTML in notes is not rendered, the same as the React Markdown component, so content can't inject markup.
    private static readonly MarkdownPipeline Markdown = new MarkdownPipelineBuilder().UsePipeTables().DisableHtml().Build();

    // A run of Devanagari text (with the spaces and punctuation inside it), for marking Hindi with lang="hi".
    [GeneratedRegex(@"[\u0900-\u097F](?:[\u0900-\u097F\u200C\u200D\s,.;:()'""\-\u2013]*[\u0900-\u097F])?")]
    private static partial Regex DevanagariRun();

    [GeneratedRegex(@">([^<]+)<")]
    private static partial Regex TextNode();

    // Shown only until the React page takes over; the site's own styles load with the app bundle.
    private const string PrerenderStyle =
        "<style>#ssr{max-width:860px;margin:0 auto;padding:24px 16px 48px;font:16px/1.65 system-ui,-apple-system,'Segoe UI',sans-serif;color:#0f172a}" +
        "#ssr a{color:#1d4ed8}#ssr h1{font-size:2rem;line-height:1.2;margin:8px 0 12px}#ssr table{border-collapse:collapse;width:100%;margin:16px 0}" +
        "#ssr th,#ssr td{border:1px solid #e2e8f0;padding:6px 10px;text-align:left}#ssr .q{border:1px solid #e2e8f0;border-radius:12px;padding:16px;margin:0 0 16px}" +
        "#ssr details{margin-top:8px}#ssr nav{font-size:.9rem;color:#475569}</style>";

    private async Task<PageMeta> ResolveJeeAsync(string path, AppDbContext db, HttpContext context, CancellationToken ct)
    {
        var canonical = path.Length > 1 ? path.TrimEnd('/') : path;
        if (string.Equals(canonical, JeePages.HubPath, StringComparison.OrdinalIgnoreCase))
        {
            return await JeeHubAsync(db, context, ct);
        }

        var page = JeePages.ByPath(canonical);
        var meta = page?.Kind switch
        {
            "test" => await JeeTestAsync(page, db, context, ct),
            "notes" => await JeeNotesAsync(page, db, context, ct),
            _ => null
        };

        return meta ?? new PageMeta($"Page not found | {SiteBrand}", "This IIT JEE page does not exist.", NoIndex: true, Status: StatusCodes.Status404NotFound);
    }

    private async Task<PageMeta> JeeHubAsync(AppDbContext db, HttpContext context, CancellationToken ct)
    {
        var available = await AvailableAsync(db, ct);
        var pages = JeePages.All.Where(p => available.Contains(p.Path)).ToList();

        var body = new StringBuilder();
        body.Append(Breadcrumbs([("Curious Minds", "/gk"), ("IIT JEE", null)]));
        body.Append($"<h1>{E(JeePages.HubHeading)}</h1>");
        body.Append("<p>Free chapter-wise notes and tests for JEE Main and Advanced, with real JEE marking (+4 / −1) and a step-by-step solution to every question in English and Hindi.</p>");
        body.Append("<p lang=\"hi\">JEE Main और Advanced के लिए मुफ़्त अध्यायवार नोट्स और टेस्ट, JEE अंकन (+4 / −1) और हर प्रश्न का अंग्रेज़ी व हिंदी में पूरा हल।</p>");
        foreach (var subject in pages.GroupBy(p => p.Subject))
        {
            body.Append($"<h2>{E(subject.Key)}</h2><ul>");
            foreach (var p in subject)
            {
                body.Append($"<li><a href=\"{p.Path}\">{E(p.Title)}</a>: {E(p.Description)}</li>");
            }

            body.Append("</ul>");
        }

        body.Append("<p>More chapters in Physics, Chemistry and Mathematics are being added. Every question here is an original practice question, not a previous year question.</p>");

        var url = Abs(context, JeePages.HubPath);
        var jsonLd = new List<object>
        {
            new Dictionary<string, object?>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "CollectionPage",
                ["name"] = JeePages.HubTitle,
                ["description"] = JeePages.HubDescription,
                ["url"] = url,
                ["inLanguage"] = new[] { "en", "hi" },
                ["isPartOf"] = new Dictionary<string, object?> { ["@type"] = "WebSite", ["name"] = SiteBrand, ["url"] = Abs(context, "/gk") },
                ["hasPart"] = pages.Select(p => new Dictionary<string, object?>
                {
                    ["@type"] = p.Kind == "test" ? "Quiz" : "Article",
                    ["name"] = p.Title,
                    ["url"] = Abs(context, p.Path)
                }).ToList()
            },
            BreadcrumbLd(context, [("Curious Minds", "/gk"), ("IIT JEE", JeePages.HubPath)])
        };

        return new PageMeta(JeePages.HubTitle, JeePages.HubDescription, TestThumbnail("index"), Canonical: JeePages.HubPath, JsonLd: jsonLd,
            Body: Prerender(JeePages.HubPath, body.ToString()));
    }

    private async Task<PageMeta?> JeeTestAsync(JeePage page, AppDbContext db, HttpContext context, CancellationToken ct)
    {
        var exam = await db.Exams.AsNoTracking().Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Slug == page.Slug && e.IsPublished, ct);
        if (exam is null)
        {
            return null;
        }

        var questions = exam.Questions.OrderBy(q => q.SortOrder).ToList();
        var notes = JeePages.Companion(page);
        var notesAvailable = notes is not null && await db.Posts.AnyAsync(p => p.Blog == BlogKeys.Gk && p.Slug == notes.Slug && p.IsPublished, ct);

        var body = new StringBuilder();
        body.Append(Breadcrumbs([("Curious Minds", "/gk"), ("IIT JEE", JeePages.HubPath), (page.Crumb, null)]));
        body.Append($"<h1>{E(exam.Title)}</h1><p>{E(exam.Description)}</p>");
        body.Append($"<ul><li>{questions.Count} questions</li><li>{exam.DurationMinutes} minutes</li><li>Marking: +{questions.FirstOrDefault()?.Marks ?? 1:0.##} correct, −{exam.NegativeMark:0.##} wrong</li><li>{(exam.Price > 0 ? $"₹{exam.Price}" : "Free")}</li></ul>");
        if (notesAvailable)
        {
            body.Append($"<p><a href=\"{notes!.Path}\">Revise first: {E(notes.Title)}</a></p>");
        }

        body.Append($"<h2>All {questions.Count} questions with answers and solutions</h2><ol>");
        foreach (var q in questions)
        {
            body.Append("<li class=\"q\">");
            body.Append($"<p><small>{E(q.Section)}</small></p>");
            if (!string.IsNullOrWhiteSpace(q.Passage))
            {
                body.Append($"<blockquote><p>{Lines(q.Passage)}</p>{(string.IsNullOrWhiteSpace(q.PassageHi) ? "" : $"<p lang=\"hi\">{Lines(q.PassageHi)}</p>")}</blockquote>");
            }

            body.Append($"<p><strong>{Lines(q.Text)}</strong></p>");
            if (!string.IsNullOrWhiteSpace(q.TextHi))
            {
                body.Append($"<p lang=\"hi\">{Lines(q.TextHi)}</p>");
            }

            body.Append("<ol type=\"A\">");
            for (var i = 0; i < q.Options.Length; i++)
            {
                var hi = q.OptionsHi.ElementAtOrDefault(i);
                var showHi = !string.IsNullOrWhiteSpace(hi) && hi != q.Options[i];
                body.Append($"<li>{E(q.Options[i])}{(showHi ? $" <span lang=\"hi\">/ {E(hi!)}</span>" : "")}</li>");
            }

            body.Append("</ol>");
            body.Append($"<details><summary>Answer and solution / उत्तर एवं हल</summary><p><strong>Answer: ({Letters[q.Answer]}) {E(q.Options.ElementAtOrDefault(q.Answer) ?? "")}</strong></p>");
            if (!string.IsNullOrWhiteSpace(q.Explanation))
            {
                body.Append($"<p>{Lines(q.Explanation)}</p>");
            }

            if (!string.IsNullOrWhiteSpace(q.ExplanationHi))
            {
                body.Append($"<p lang=\"hi\">{Lines(q.ExplanationHi)}</p>");
            }

            body.Append("</details></li>");
        }

        body.Append("</ol>");
        if (notesAvailable)
        {
            body.Append($"<p><a href=\"{notes!.Path}\">Read the {E(page.Topic)} notes</a> · <a href=\"{JeePages.HubPath}\">All IIT JEE chapters</a></p>");
        }

        var url = Abs(context, page.Path);
        var jsonLd = new List<object>
        {
            new Dictionary<string, object?>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "Quiz",
                ["name"] = exam.Title,
                ["description"] = page.Description,
                ["url"] = url,
                ["about"] = new Dictionary<string, object?> { ["@type"] = "Thing", ["name"] = page.Topic },
                ["educationalLevel"] = "JEE Main",
                ["inLanguage"] = new[] { "en", "hi" },
                ["numberOfQuestions"] = questions.Count,
                ["timeRequired"] = $"PT{exam.DurationMinutes}M",
                ["isAccessibleForFree"] = exam.Price == 0,
                ["dateModified"] = exam.UpdatedAt.ToString("yyyy-MM-dd"),
                ["provider"] = new Dictionary<string, object?> { ["@type"] = "Organization", ["name"] = SiteBrand, ["url"] = Abs(context, "/gk") }
            },
            BreadcrumbLd(context, [("Curious Minds", "/gk"), ("IIT JEE", JeePages.HubPath), (page.Crumb, page.Path)])
        };

        return new PageMeta(page.Title, page.Description, TestThumbnail(page.Slug), Canonical: page.Path, JsonLd: jsonLd,
            Body: Prerender(page.Path, body.ToString()));
    }

    private async Task<PageMeta?> JeeNotesAsync(JeePage page, AppDbContext db, HttpContext context, CancellationToken ct)
    {
        var post = await db.Posts.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Blog == BlogKeys.Gk && p.Slug == page.Slug && p.IsPublished, ct);
        if (post is null)
        {
            return null;
        }

        var test = JeePages.Companion(page);
        var testAvailable = test is not null && await db.Exams.AnyAsync(e => e.Slug == test.Slug && e.IsPublished, ct);
        var testLink = testAvailable ? $"<p><a href=\"{test!.Path}\">Practise now: {E(test.Title)}</a></p>" : "";

        var html = Markdig.Markdown.ToHtml(JeePages.RewriteOldLinks(post.Content), Markdown);
        var body = new StringBuilder();
        body.Append(Breadcrumbs([("Curious Minds", "/gk"), ("IIT JEE", JeePages.HubPath), (page.Crumb, null)]));
        body.Append($"<article><h1>{E(post.Title)}</h1>");
        if (!string.IsNullOrWhiteSpace(post.Excerpt))
        {
            body.Append($"<p>{E(post.Excerpt)}</p>");
        }

        body.Append($"<p><small>{E(string.IsNullOrWhiteSpace(post.Author) ? SiteBrand : post.Author)} · Updated {post.UpdatedAt:d MMMM yyyy}</small></p>");
        body.Append(testLink);
        body.Append(MarkHindi(html));
        body.Append(testLink);
        body.Append("</article>");

        var author = string.IsNullOrWhiteSpace(post.Author) ? SiteBrand : post.Author;
        var image = Abs(context, string.IsNullOrWhiteSpace(post.CoverImageUrl) ? TestThumbnail("index") : post.CoverImageUrl);
        var jsonLd = new List<object>
        {
            new Dictionary<string, object?>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "Article",
                ["headline"] = post.Title.Length <= 110 ? post.Title : post.Title[..110],
                ["description"] = page.Description,
                ["image"] = image,
                ["author"] = new Dictionary<string, object?>
                {
                    ["@type"] = author == SiteBrand ? "Organization" : "Person",
                    ["name"] = author,
                    ["url"] = Abs(context, "/gk")
                },
                ["publisher"] = new Dictionary<string, object?> { ["@type"] = "Organization", ["name"] = SiteBrand, ["url"] = Abs(context, "/gk") },
                ["datePublished"] = post.PublishedAt.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
                ["dateModified"] = post.UpdatedAt.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
                ["mainEntityOfPage"] = Abs(context, page.Path),
                ["inLanguage"] = "en"
            },
            BreadcrumbLd(context, [("Curious Minds", "/gk"), ("IIT JEE", JeePages.HubPath), (page.Crumb, page.Path)])
        };

        return new PageMeta(page.Title, page.Description, image, "article", Canonical: page.Path, JsonLd: jsonLd,
            Body: Prerender(page.Path, body.ToString()));
    }

    /// <summary>The catalog paths whose test or post exists and is published (a page without content isn't linked).</summary>
    private static async Task<HashSet<string>> AvailableAsync(AppDbContext db, CancellationToken ct)
    {
        var testSlugs = JeePages.All.Where(p => p.Kind == "test").Select(p => p.Slug).ToList();
        var noteSlugs = JeePages.All.Where(p => p.Kind == "notes").Select(p => p.Slug).ToList();
        var tests = await db.Exams.Where(e => e.IsPublished && testSlugs.Contains(e.Slug)).Select(e => e.Slug).ToListAsync(ct);
        var notes = await db.Posts.Where(p => p.Blog == BlogKeys.Gk && p.IsPublished && noteSlugs.Contains(p.Slug)).Select(p => p.Slug).ToListAsync(ct);
        return JeePages.All.Where(p => (p.Kind == "test" ? tests : notes).Contains(p.Slug)).Select(p => p.Path).ToHashSet();
    }

    private static string Prerender(string path, string html) =>
        $"{PrerenderStyle}<div id=\"ssr\" data-path=\"{path}\"><main>{html}</main></div>";

    private static string Breadcrumbs(IReadOnlyList<(string Name, string? Path)> items) =>
        "<nav aria-label=\"Breadcrumb\">" + string.Join(" › ", items.Select(i => i.Path is null ? E(i.Name) : $"<a href=\"{i.Path}\">{E(i.Name)}</a>")) + "</nav>";

    private object BreadcrumbLd(HttpContext context, IReadOnlyList<(string Name, string Path)> items) => new Dictionary<string, object?>
    {
        ["@context"] = "https://schema.org",
        ["@type"] = "BreadcrumbList",
        ["itemListElement"] = items.Select((item, i) => new Dictionary<string, object?>
        {
            ["@type"] = "ListItem",
            ["position"] = i + 1,
            ["name"] = item.Name,
            ["item"] = Abs(context, item.Path)
        }).ToList()
    };

    private string Abs(HttpContext context, string path) => path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? path : AbsoluteUrl(context, path);

    private static string E(string s) => WebUtility.HtmlEncode(s);

    private static string Lines(string s) => E(s.Trim()).ReplaceLineEndings("<br>");

    /// <summary>Wraps each run of Hindi (Devanagari) text in rendered HTML in &lt;span lang="hi"&gt;.</summary>
    private static string MarkHindi(string html) =>
        TextNode().Replace(html, m => ">" + DevanagariRun().Replace(m.Groups[1].Value, r => $"<span lang=\"hi\">{r.Value}</span>") + "<");
}
