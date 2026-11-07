using System.Text.Json;

namespace MinnieWorld.Web.Services.Cbse;

/// <summary>
/// The CBSE section (Class 6: Mathematics, English, Personality Development): every chapter file under Data/Seed/cbse (or Cbse:ContentPath), validated and indexed.
/// Files are read once; in Development they are re-read whenever one changes, so edits show up on refresh.
/// An invalid file is skipped with a warning in the log, so one bad file never takes the section down.
/// </summary>
public sealed class CbseCatalog(IWebHostEnvironment env, IConfiguration config, ILogger<CbseCatalog> logger)
{
    public const string HubPath = "/cbse";

    /// <summary>The classes offered. Only Class 6 for now; add a number here (and its chapter files) to open another class.</summary>
    public static readonly int[] Classes = [6];

    public static readonly IReadOnlyList<CbseSubject> Subjects =
    [
        new("mathematics", "Mathematics", "गणित"),
        new("english", "English", "अंग्रेज़ी"),
        new("personality-development", "Personality Development", "व्यक्तित्व विकास")
    ];

    /// <summary>Where the section starts. With a single class, /cbse redirects straight to that class's page.</summary>
    public static string EntryPath => Classes.Length == 1 ? ClassPath(Classes[0]) : HubPath;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
    private readonly Lock _gate = new();
    private IReadOnlyList<CbseChapter> _chapters = [];
    private DateTime _loadedStamp = DateTime.MinValue;
    private bool _loaded;

    public string Folder => config["Cbse:ContentPath"] is { Length: > 0 } custom ? custom : Path.Combine(env.ContentRootPath, "Data", "Seed", "cbse");

    public static CbseSubject? Subject(string key) => Subjects.FirstOrDefault(s => s.Key == key);

    public static string ClassPath(int n) => $"{HubPath}/class-{n}";
    public static string SubjectPath(int n, string subject) => $"{ClassPath(n)}/{subject}";
    public static string ChapterPath(CbseChapter c) => $"{SubjectPath(c.Class, c.Subject)}/{c.Slug}";

    /// <summary>Parses "class-6" into 6, or null for a class that isn't offered.</summary>
    public static int? ParseClass(string segment) =>
        segment.StartsWith("class-", StringComparison.Ordinal) && int.TryParse(segment[6..], out var n) && Classes.Contains(n) && segment == $"class-{n}" ? n : null;

    public IReadOnlyList<CbseChapter> All()
    {
        lock (_gate)
        {
            var stamp = env.IsDevelopment() || !_loaded ? Stamp() : _loadedStamp;
            if (!_loaded || stamp != _loadedStamp)
            {
                _chapters = Load();
                _loadedStamp = stamp;
                _loaded = true;
            }

            return _chapters;
        }
    }

    /// <summary>A subject's chapters in book order.</summary>
    public IReadOnlyList<CbseChapter> Chapters(int n, string subject) =>
        All().Where(c => c.Class == n && c.Subject == subject).OrderBy(c => c.Number).ToList();

    public CbseChapter? Chapter(int n, string subject, string slug) =>
        All().FirstOrDefault(c => c.Class == n && c.Subject == subject && c.Slug == slug);

    /// <summary>The chapters before and after this one in the same subject, for Previous / Next links.</summary>
    public (CbseChapter? Previous, CbseChapter? Next) Neighbours(CbseChapter chapter)
    {
        var list = Chapters(chapter.Class, chapter.Subject);
        var i = list.ToList().FindIndex(c => c.Slug == chapter.Slug);
        return (i > 0 ? list[i - 1] : null, i >= 0 && i < list.Count - 1 ? list[i + 1] : null);
    }

    /// <summary>The date a chapter last changed: its "updated" field, or else the file's modified date.</summary>
    public static DateOnly? Modified(CbseChapter c) => DateOnly.TryParseExact(c.Updated, "yyyy-MM-dd", out var d) ? d : null;

    private DateTime Stamp()
    {
        if (!Directory.Exists(Folder)) return DateTime.MinValue;
        var files = Files();
        return files.Count == 0 ? DateTime.MinValue : new DateTime(files.Max(f => File.GetLastWriteTimeUtc(f).Ticks) + files.Count);
    }

    // The schema folder holds the JSON Schemas (documentation), not chapters.
    private List<string> Files() => Directory.EnumerateFiles(Folder, "*.json", SearchOption.AllDirectories)
        .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}schema{Path.DirectorySeparatorChar}") && !f.Contains("/schema/"))
        .Order().ToList();

    private List<CbseChapter> Load()
    {
        var result = new List<CbseChapter>();
        if (!Directory.Exists(Folder)) return result;

        var subjectKeys = Subjects.Select(s => s.Key).ToList();
        foreach (var file in Files())
        {
            var name = Path.GetRelativePath(Folder, file);
            CbseChapter? chapter;
            try
            {
                chapter = JsonSerializer.Deserialize<CbseChapter>(File.ReadAllText(file), Json);
            }
            catch (JsonException e)
            {
                logger.LogWarning("CBSE: skipped {File}: {Error}", name, e.Message);
                continue;
            }

            if (chapter is null) continue;
            var errors = CbseValidator.Validate(chapter, subjectKeys);
            if (result.Any(c => c.Class == chapter.Class && c.Subject == chapter.Subject && c.Slug == chapter.Slug))
                errors.Add($"another file already uses class {chapter.Class} / {chapter.Subject} / {chapter.Slug}");

            if (errors.Count > 0)
            {
                logger.LogWarning("CBSE: skipped {File}: {Errors}", name, string.Join("; ", errors));
                continue;
            }

            result.Add(chapter);
        }

        logger.LogInformation("CBSE: loaded {Count} chapters from {Folder}", result.Count, Folder);
        return result;
    }
}
