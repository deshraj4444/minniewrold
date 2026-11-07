using MinnieWorld.Web.Services.Cbse;

namespace MinnieWorld.Web.Endpoints;

/// <summary>Read-only API for the CBSE section. Content comes from the chapter files via <see cref="CbseCatalog"/>.</summary>
public static class CbseEndpoints
{
    public static void MapCbseEndpoints(this WebApplication app)
    {
        var g = app.MapGroup("/api/cbse");

        // Every class with its subjects and how many chapters each has (the /cbse and /cbse/class-{n} hubs).
        g.MapGet("/", (CbseCatalog catalog) => Results.Ok(new
        {
            Classes = CbseCatalog.Classes.Select(n => new
            {
                Class = n,
                Subjects = CbseCatalog.Subjects.Select(s => new { s.Key, s.Name, s.NameHi, Chapters = catalog.Chapters(n, s.Key).Count })
            })
        }));

        // A subject's chapters in book order (the /cbse/class-{n}/{subject} hub).
        g.MapGet("/class-{n:int}/{subject}", (int n, string subject, CbseCatalog catalog) =>
        {
            var s = CbseCatalog.Subject(subject);
            if (s is null || !CbseCatalog.Classes.Contains(n)) return Results.NotFound();
            return Results.Ok(new
            {
                Class = n,
                Subject = s,
                Chapters = catalog.Chapters(n, subject).Select(c => new { c.Number, c.Slug, c.Title, c.TitleHi, c.Summary, c.SummaryHi, Path = CbseCatalog.ChapterPath(c) })
            });
        });

        // A whole chapter: notes, games, practice questions, FAQs, and the previous / next chapter.
        g.MapGet("/class-{n:int}/{subject}/{slug}", (int n, string subject, string slug, CbseCatalog catalog) =>
        {
            var chapter = catalog.Chapter(n, subject, slug);
            if (chapter is null) return Results.NotFound();
            var (previous, next) = catalog.Neighbours(chapter);
            return Results.Ok(new
            {
                Chapter = chapter,
                Subject = CbseCatalog.Subject(subject),
                Path = CbseCatalog.ChapterPath(chapter),
                Previous = previous is null ? null : new { previous.Number, previous.Title, Path = CbseCatalog.ChapterPath(previous) },
                Next = next is null ? null : new { next.Number, next.Title, Path = CbseCatalog.ChapterPath(next) }
            });
        });
    }
}
