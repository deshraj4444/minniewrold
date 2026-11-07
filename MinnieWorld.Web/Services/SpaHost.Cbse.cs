using System.Text;
using System.Text.Json;
using MinnieWorld.Web.Services.Cbse;

namespace MinnieWorld.Web.Services;

/// <summary>
/// The CBSE pages (Class 6 for now): /cbse/class-{n}, /cbse/class-{n}/{subject} and /cbse/class-{n}/{subject}/{chapter}.
/// Like the JEE pages, the notes, practice questions and FAQs are rendered here into the first HTML response, and the
/// React page (client/src/gk/cbse) replaces them once it loads, so the two must show the same content.
/// </summary>
public partial class SpaHost
{
    private static readonly string[] OptionLetters = ["A", "B", "C", "D"];

    private PageMeta ResolveCbse(string[] parts, HttpContext context)
    {
        var classNumber = parts.Length >= 2 ? CbseCatalog.ParseClass(parts[1]) : null;
        if (parts.Length == 1) return CbseCatalog.Classes.Length == 1 ? CbseClassHub(CbseCatalog.Classes[0], context) : CbseHub(context);
        if (classNumber is not { } n) return CbseNotFound();
        if (parts.Length == 2) return CbseClassHub(n, context);

        var subject = CbseCatalog.Subject(parts[2]);
        if (subject is null) return CbseNotFound();
        if (parts.Length == 3) return CbseSubjectHub(n, subject, context);

        var chapter = parts.Length == 4 ? cbse.Chapter(n, subject.Key, parts[3]) : null;
        return chapter is null ? CbseNotFound() : CbseChapterPage(chapter, subject, context);
    }

    private static PageMeta CbseNotFound() =>
        new($"Page not found | {SiteBrand}", "This CBSE page does not exist.", NoIndex: true, Status: StatusCodes.Status404NotFound);

    private PageMeta CbseHub(HttpContext context)
    {
        const string title = "CBSE Notes, Games & Practice (English + Hindi)";
        const string description = "Free CBSE chapter notes, learning games and practice questions with solutions, in English and Hindi. No login needed.";
        const string heading = "CBSE: notes, games and practice";

        var body = new StringBuilder();
        body.Append(Breadcrumbs([("Curious Minds", "/gk"), ("CBSE", null)]));
        body.Append($"<h1>{E(heading)}</h1>");
        body.Append("<p>Short concept notes, learning games and practice questions with solutions for every chapter, in English and Hindi. Free, and no login needed.</p>");
        body.Append("<p lang=\"hi\">हर अध्याय के लिए छोटे नोट्स, सीखने वाले खेल और हल सहित अभ्यास प्रश्न, अंग्रेज़ी और हिंदी में। मुफ़्त, लॉगिन की ज़रूरत नहीं।</p><ul>");
        foreach (var n in CbseCatalog.Classes)
        {
            var count = cbse.All().Count(c => c.Class == n);
            body.Append($"<li><a href=\"{CbseCatalog.ClassPath(n)}\">Class {n}</a>: {(count > 0 ? $"{count} chapter{(count == 1 ? "" : "s")}" : "coming soon")}</li>");
        }

        body.Append("</ul>");
        var jsonLd = new List<object>
        {
            CollectionLd(context, title, description, CbseCatalog.HubPath,
                CbseCatalog.Classes.Where(n => cbse.All().Any(c => c.Class == n)).Select(n => ($"Class {n}", CbseCatalog.ClassPath(n)))),
            BreadcrumbLd(context, [("Curious Minds", "/gk"), ("CBSE", CbseCatalog.HubPath)])
        };
        // Until the first chapter is published the hub is an empty page, so keep it out of search results.
        return new PageMeta(title, description, TestThumbnail("index"), NoIndex: cbse.All().Count == 0, Canonical: CbseCatalog.HubPath, JsonLd: jsonLd,
            Body: Prerender(CbseCatalog.HubPath, body.ToString()));
    }

    private PageMeta CbseClassHub(int n, HttpContext context)
    {
        var path = CbseCatalog.ClassPath(n);
        var title = Fit($"CBSE Class {n} Maths, English & Personality Development", $"CBSE Class {n}: Notes, Games & Practice");
        var description = $"Free CBSE Class {n} Mathematics, English and Personality Development: chapter notes, learning games and practice with solutions, in English and Hindi.";
        var subjects = CbseCatalog.Subjects.Select(s => (Subject: s, Count: cbse.Chapters(n, s.Key).Count)).ToList();

        var body = new StringBuilder();
        body.Append(Breadcrumbs([("Curious Minds", "/gk"), ($"CBSE Class {n}", null)]));
        body.Append($"<h1>CBSE Class {n}: notes, games and practice</h1>");
        body.Append("<p>Short concept notes, learning games and practice questions with solutions, in English and Hindi. Free, and no login needed.</p>");
        body.Append("<p lang=\"hi\">छोटे नोट्स, सीखने वाले खेल और हल सहित अभ्यास प्रश्न, अंग्रेज़ी और हिंदी में। मुफ़्त, लॉगिन की ज़रूरत नहीं।</p><ul>");
        foreach (var (s, count) in subjects)
        {
            body.Append($"<li><a href=\"{CbseCatalog.SubjectPath(n, s.Key)}\">{E(s.Name)}</a> <span lang=\"hi\">({E(s.NameHi)})</span>: {(count > 0 ? $"{count} chapter{(count == 1 ? "" : "s")}" : "coming soon")}</li>");
        }

        body.Append("</ul>");
        var crumbs = new List<(string, string)> { ("Curious Minds", "/gk"), ($"CBSE Class {n}", path) };
        var jsonLd = new List<object>
        {
            CollectionLd(context, title, description, path, subjects.Where(s => s.Count > 0).Select(s => (s.Subject.Name, CbseCatalog.SubjectPath(n, s.Subject.Key)))),
            BreadcrumbLd(context, crumbs)
        };
        // A class with no chapters yet is a thin page: keep it out of search results until it has content.
        return new PageMeta(title, description, TestThumbnail("index"), NoIndex: subjects.All(s => s.Count == 0), Canonical: path, JsonLd: jsonLd,
            Body: Prerender(path, body.ToString()));
    }

    private PageMeta CbseSubjectHub(int n, CbseSubject subject, HttpContext context)
    {
        var path = CbseCatalog.SubjectPath(n, subject.Key);
        var title = Fit($"CBSE Class {n} {subject.Name}: Chapter Notes, Games & Practice", $"CBSE Class {n} {subject.Name}: Notes, Games & Practice");
        var description = $"Free CBSE Class {n} {subject.Name} chapter-wise notes, learning games and practice questions with solutions, in English and Hindi.";
        var chapters = cbse.Chapters(n, subject.Key);

        var body = new StringBuilder();
        body.Append(Breadcrumbs([("Curious Minds", "/gk"), ($"CBSE Class {n}", CbseCatalog.ClassPath(n)), (subject.Name, null)]));
        body.Append($"<h1>CBSE Class {n} {E(subject.Name)} <span lang=\"hi\">({E(subject.NameHi)})</span>: chapters</h1>");
        if (chapters.Count == 0)
        {
            body.Append("<p>Chapters are on their way. Check back soon.</p>");
        }
        else
        {
            body.Append("<ol>");
            foreach (var c in chapters)
            {
                body.Append($"<li><a href=\"{CbseCatalog.ChapterPath(c)}\">Chapter {c.Number}: {E(c.Title)}</a>{(string.IsNullOrWhiteSpace(c.TitleHi) ? "" : $" <span lang=\"hi\">({E(c.TitleHi)})</span>")}. {E(c.Summary)}</li>");
            }

            body.Append("</ol>");
        }

        var jsonLd = new List<object>
        {
            CollectionLd(context, title, description, path, chapters.Select(c => ($"Chapter {c.Number}: {c.Title}", CbseCatalog.ChapterPath(c)))),
            BreadcrumbLd(context, [("Curious Minds", "/gk"), ($"CBSE Class {n}", CbseCatalog.ClassPath(n)), (subject.Name, path)])
        };
        return new PageMeta(title, description, TestThumbnail("index"), NoIndex: chapters.Count == 0, Canonical: path, JsonLd: jsonLd,
            Body: Prerender(path, body.ToString()));
    }

    /// <summary>The H1 the brief asks for. The React page uses the same wording (CbseChapter.jsx).</summary>
    public static string CbseHeading(CbseChapter c, CbseSubject s) => $"Class {c.Class} {s.Name} Chapter {c.Number}: {c.Title} – Notes, Games & Practice";

    private PageMeta CbseChapterPage(CbseChapter c, CbseSubject s, HttpContext context)
    {
        var path = CbseCatalog.ChapterPath(c);
        var title = Fit($"Class {c.Class} {s.Name} Ch {c.Number}: {c.Title} – Notes & Practice", $"Class {c.Class} {s.Name} Ch {c.Number}: {c.Title}");
        var description = c.Summary.Length <= 155 ? c.Summary : c.Summary[..152].TrimEnd() + "…";
        var (previous, next) = cbse.Neighbours(c);

        var body = new StringBuilder();
        body.Append(Breadcrumbs([("Curious Minds", "/gk"), ($"CBSE Class {c.Class}", CbseCatalog.ClassPath(c.Class)),
            (s.Name, CbseCatalog.SubjectPath(c.Class, s.Key)), ($"Chapter {c.Number}", null)]));
        body.Append($"<article><h1>{E(CbseHeading(c, s))}</h1>");
        if (!string.IsNullOrWhiteSpace(c.TitleHi)) body.Append($"<p lang=\"hi\"><strong>{E(c.TitleHi)}</strong></p>");
        body.Append($"<p>{E(c.Summary)}</p>");
        if (!string.IsNullOrWhiteSpace(c.SummaryHi)) body.Append($"<p lang=\"hi\">{E(c.SummaryHi)}</p>");
        if (!string.IsNullOrWhiteSpace(c.Book)) body.Append($"<p><small>Textbook: {E(c.Book)}</small></p>");

        body.Append("<section><h2>Concept notes</h2>");
        body.Append(MarkHindi(Markdig.Markdown.ToHtml(c.Notes, Markdown)));
        if (!string.IsNullOrWhiteSpace(c.NotesHi))
        {
            body.Append($"<div lang=\"hi\"><h3>हिंदी में नोट्स</h3>{Markdig.Markdown.ToHtml(c.NotesHi, Markdown)}</div>");
        }

        body.Append("</section>");

        if (c.Games.Count > 0)
        {
            body.Append("<section><h2>Learning games</h2><ul>");
            foreach (var g in c.Games)
            {
                var gTitle = Prop(g, "title");
                var gTitleHi = Prop(g, "titleHi");
                var how = Prop(g, "instructions");
                body.Append($"<li><strong>{E(gTitle ?? "Game")}</strong>{(gTitleHi is null ? "" : $" <span lang=\"hi\">({E(gTitleHi)})</span>")}{(how is null ? "" : $": {E(how)}")}</li>");
            }

            body.Append("</ul></section>");
        }

        if (c.Questions.Count > 0)
        {
            body.Append($"<section><h2>{c.Questions.Count} practice questions with solutions</h2><ol>");
            foreach (var q in c.Questions)
            {
                body.Append("<li class=\"q\">");
                if (q.Kind == "case")
                {
                    body.Append($"<p><small>Case-based question</small></p><blockquote><p>{Lines(q.Case ?? "")}</p>{(string.IsNullOrWhiteSpace(q.CaseHi) ? "" : $"<p lang=\"hi\">{Lines(q.CaseHi)}</p>")}</blockquote>");
                }

                body.Append($"<p><strong>{Lines(q.Text)}</strong></p>");
                if (!string.IsNullOrWhiteSpace(q.TextHi)) body.Append($"<p lang=\"hi\">{Lines(q.TextHi)}</p>");
                body.Append("<ol type=\"A\">");
                for (var i = 0; i < q.Options.Count; i++)
                {
                    var hi = q.OptionsHi?.ElementAtOrDefault(i);
                    body.Append($"<li>{E(q.Options[i])}{(!string.IsNullOrWhiteSpace(hi) && hi != q.Options[i] ? $" <span lang=\"hi\">/ {E(hi)}</span>" : "")}</li>");
                }

                body.Append($"</ol><details><summary>Answer and solution / उत्तर एवं हल</summary><p><strong>Answer: ({OptionLetters[q.Answer]}) {E(q.Options[q.Answer])}</strong></p><p>{Lines(q.Explanation)}</p>");
                if (!string.IsNullOrWhiteSpace(q.ExplanationHi)) body.Append($"<p lang=\"hi\">{Lines(q.ExplanationHi)}</p>");
                body.Append("</details></li>");
            }

            body.Append("</ol></section>");
        }

        if (c.Faqs.Count > 0)
        {
            body.Append("<section><h2>Frequently asked questions</h2>");
            foreach (var f in c.Faqs)
            {
                body.Append($"<h3>{E(f.Q)}</h3><p>{Lines(f.A)}</p>");
                if (!string.IsNullOrWhiteSpace(f.QHi)) body.Append($"<p lang=\"hi\"><strong>{E(f.QHi)}</strong></p>");
                if (!string.IsNullOrWhiteSpace(f.AHi)) body.Append($"<p lang=\"hi\">{Lines(f.AHi)}</p>");
            }

            body.Append("</section>");
        }

        body.Append("<nav aria-label=\"Chapters\"><p>");
        if (previous is not null) body.Append($"<a href=\"{CbseCatalog.ChapterPath(previous)}\" rel=\"prev\">← Chapter {previous.Number}: {E(previous.Title)}</a> ");
        body.Append($"<a href=\"{CbseCatalog.SubjectPath(c.Class, s.Key)}\">All Class {c.Class} {E(s.Name)} chapters</a>");
        if (next is not null) body.Append($" <a href=\"{CbseCatalog.ChapterPath(next)}\" rel=\"next\">Chapter {next.Number}: {E(next.Title)} →</a>");
        body.Append("</p></nav></article>");

        var modified = CbseCatalog.Modified(c);
        var resource = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "LearningResource",
            ["name"] = CbseHeading(c, s),
            ["description"] = description,
            ["url"] = Abs(context, path),
            ["educationalLevel"] = $"Class {c.Class}",
            ["about"] = new Dictionary<string, object?> { ["@type"] = "Thing", ["name"] = c.Title },
            ["inLanguage"] = new[] { "en", "hi" },
            ["learningResourceType"] = new[] { "Concept notes", "Educational game", "Practice questions" },
            ["teaches"] = c.Title,
            ["isAccessibleForFree"] = true,
            ["provider"] = new Dictionary<string, object?> { ["@type"] = "Organization", ["name"] = SiteBrand, ["url"] = Abs(context, "/gk") }
        };
        if (modified is { } m) resource["dateModified"] = m.ToString("yyyy-MM-dd");

        var jsonLd = new List<object>
        {
            resource,
            BreadcrumbLd(context, [("Curious Minds", "/gk"), ($"CBSE Class {c.Class}", CbseCatalog.ClassPath(c.Class)),
                (s.Name, CbseCatalog.SubjectPath(c.Class, s.Key)), ($"Chapter {c.Number}: {c.Title}", path)])
        };
        return new PageMeta(title, description, TestThumbnail("index"), "article", Canonical: path, JsonLd: jsonLd, Body: Prerender(path, body.ToString()));
    }

    private object CollectionLd(HttpContext context, string title, string description, string path, IEnumerable<(string Name, string Path)> parts) =>
        new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "CollectionPage",
            ["name"] = title,
            ["description"] = description,
            ["url"] = Abs(context, path),
            ["inLanguage"] = new[] { "en", "hi" },
            ["isPartOf"] = new Dictionary<string, object?> { ["@type"] = "WebSite", ["name"] = SiteBrand, ["url"] = Abs(context, "/gk") },
            ["hasPart"] = parts.Select(p => new Dictionary<string, object?> { ["@type"] = "WebPage", ["name"] = p.Name, ["url"] = Abs(context, p.Path) }).ToList()
        };

    /// <summary>The long title if it fits in 60 characters, else the short one (cut if even that is too long).</summary>
    private static string Fit(string full, string shorter) =>
        full.Length <= 60 ? full : shorter.Length <= 60 ? shorter : shorter[..57].TrimEnd() + "…";

    private static string? Prop(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(v.GetString()) ? v.GetString() : null;
}
