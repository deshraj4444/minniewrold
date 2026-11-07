namespace MinnieWorld.Web.Services.Seo;

/// <summary>One indexable page of the IIT JEE section: a chapter test or a chapter's notes.</summary>
/// <param name="Path">The canonical URL path, e.g. /jee/physics/kinematics-test.</param>
/// <param name="Kind">"test" (an exam, by slug) or "notes" (a Curious Minds post, by slug).</param>
/// <param name="Title">The &lt;title&gt; tag. Keep it under 60 characters.</param>
/// <param name="Description">The meta description. Keep it under 155 characters.</param>
/// <param name="Crumb">The short name used for this page in breadcrumbs.</param>
public sealed record JeePage(string Path, string Kind, string Slug, string Subject, string Topic, string Title, string Description, string Crumb);

/// <summary>
/// The IIT JEE section lives at /jee. This catalog is the list of its pages, used for routing, the server-rendered
/// head and body, the sitemap and the 301s from the old /gk URLs. Keep client/src/gk/jee.js in step with it.
/// </summary>
public static class JeePages
{
    public const string HubPath = "/jee";
    public const string HubTitle = "IIT JEE Main Free Notes & Chapter Tests (English + Hindi)";
    public const string HubDescription = "Free JEE Main and Advanced chapter notes and tests with +4/−1 marking and step-by-step solutions in English and Hindi. Start with Physics: Kinematics.";
    public const string HubHeading = "IIT JEE preparation: free notes and chapter tests";

    public static readonly IReadOnlyList<JeePage> All =
    [
        new("/jee/physics/kinematics-test", "test", "jee-main-physics-kinematics-chapter-test", "Physics", "Kinematics",
            "JEE Main Kinematics Test: 30 Questions with Solutions",
            "Free 30-question JEE Main Physics Kinematics chapter test with +4/−1 marking, 60 minutes, and step-by-step solutions in English and Hindi.",
            "Kinematics test"),
        new("/jee/physics/kinematics-notes", "notes", "jee-physics-kinematics-notes", "Physics", "Kinematics",
            "JEE Physics Kinematics Notes: Formulas, Graphs, Projectiles",
            "Kinematics notes for JEE Main & Advanced: equations of motion, graphs, projectile and relative motion, circular motion and common mistakes.",
            "Kinematics notes")
    ];

    public static JeePage? ByPath(string path) =>
        All.FirstOrDefault(p => string.Equals(p.Path, path, StringComparison.OrdinalIgnoreCase));

    /// <summary>The notes for a test's chapter, or the test for a chapter's notes.</summary>
    public static JeePage? Companion(JeePage page) =>
        All.FirstOrDefault(p => p.Kind != page.Kind && p.Subject == page.Subject && p.Topic == page.Topic);

    /// <summary>Free JEE chapter tests publish every question with its solution, on the page and through the API.</summary>
    public static bool HasPublicSolutions(string slug) => All.Any(p => p.Kind == "test" && p.Slug == slug);

    /// <summary>The canonical path an old or non-canonical URL should be 301-redirected to, or null.</summary>
    public static string? RedirectFor(string path)
    {
        var trimmed = path.Length > 1 ? path.TrimEnd('/') : path;
        var lower = trimmed.ToLowerInvariant();

        string? target = lower switch
        {
            "/gk/jee" or "/gk/exams/jee-main" => HubPath,
            HubPath => HubPath,
            _ => null
        };

        foreach (var p in All)
        {
            var old = p.Kind == "test" ? $"/gk/tests/{p.Slug}" : $"/gk/posts/{p.Slug}";
            if (lower == old || lower == p.Path)
            {
                target = p.Path;
            }
        }

        // Redirect when the URL is old, has a trailing slash or differs in case from the canonical one.
        return target is not null && !string.Equals(target, path, StringComparison.Ordinal) ? target : null;
    }

    /// <summary>Rewrites links to the old /gk URLs inside content written before the move to /jee.</summary>
    public static string RewriteOldLinks(string text)
    {
        foreach (var p in All)
        {
            text = text.Replace(p.Kind == "test" ? $"/gk/tests/{p.Slug}" : $"/gk/posts/{p.Slug}", p.Path);
        }

        return text.Replace("/gk/jee", HubPath);
    }
}
