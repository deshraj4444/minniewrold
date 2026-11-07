using System.Text.Json;
using System.Text.RegularExpressions;

namespace MinnieWorld.Web.Services.Cbse;

/// <summary>A school subject. The key is the URL segment, e.g. /cbse/class-6/mathematics.</summary>
public sealed record CbseSubject(string Key, string Name, string NameHi);

/// <summary>
/// One chapter page (/cbse/class-{n}/{subject}/{slug}), loaded from Data/Seed/cbse/**/*.json. The JSON Schemas in
/// Data/Seed/cbse/schema describe the file format; <see cref="CbseValidator"/> enforces the rules that matter at runtime.
/// </summary>
public sealed record CbseChapter
{
    public int Class { get; init; }
    public string Subject { get; init; } = "";
    public int Number { get; init; }
    public string Slug { get; init; } = "";
    public string Title { get; init; } = "";
    public string? TitleHi { get; init; }
    /// <summary>The textbook, e.g. "Ganita Manjari (NCERT, 2026-27)".</summary>
    public string? Book { get; init; }
    /// <summary>One or two sentences: the page intro and the meta description.</summary>
    public string Summary { get; init; } = "";
    public string? SummaryHi { get; init; }
    /// <summary>The date the content was last changed (yyyy-MM-dd), for the sitemap and dateModified.</summary>
    public string? Updated { get; init; }
    /// <summary>Concept notes in Markdown (English), and optionally in Hindi.</summary>
    public string Notes { get; init; } = "";
    public string? NotesHi { get; init; }
    /// <summary>Each game is passed to the client as-is; its "type" picks the component (see CbseValidator.GameTypes).</summary>
    public List<JsonElement> Games { get; init; } = [];
    public List<CbseQuestion> Questions { get; init; } = [];
    public List<CbseFaq> Faqs { get; init; } = [];
}

/// <param name="Kind">"mcq" or "case" (a case-based / competency question with a short case to read first).</param>
public sealed record CbseQuestion(
    string Kind, string? Case, string? CaseHi, string Text, string? TextHi,
    List<string> Options, List<string>? OptionsHi, int Answer, string Explanation, string? ExplanationHi);

public sealed record CbseFaq(string Q, string? QHi, string A, string? AHi);

public static partial class CbseValidator
{
    public static readonly string[] GameTypes = ["dragSort", "matchPairs", "timeline", "labelDiagram", "numberLine", "speedQuiz"];

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();

    /// <summary>The problems that stop a chapter from being published (an empty list means it's valid).</summary>
    public static List<string> Validate(CbseChapter c, IReadOnlyCollection<string> subjects)
    {
        var errors = new List<string>();
        if (!CbseCatalog.Classes.Contains(c.Class)) errors.Add($"class must be one of {string.Join(", ", CbseCatalog.Classes)} (was {c.Class})");
        if (!subjects.Contains(c.Subject)) errors.Add($"unknown subject \"{c.Subject}\" (use one of {string.Join(", ", subjects)})");
        if (c.Number < 1) errors.Add("number must be 1 or more");
        if (!SlugPattern().IsMatch(c.Slug)) errors.Add($"slug \"{c.Slug}\" must be lowercase words joined by hyphens");
        if (string.IsNullOrWhiteSpace(c.Title)) errors.Add("title is required");
        if (string.IsNullOrWhiteSpace(c.Summary)) errors.Add("summary is required");
        if (string.IsNullOrWhiteSpace(c.Notes)) errors.Add("notes are required");
        if (c.Updated is not null && !DateOnly.TryParseExact(c.Updated, "yyyy-MM-dd", out _)) errors.Add("updated must be yyyy-MM-dd");

        for (var i = 0; i < c.Questions.Count; i++)
        {
            var q = c.Questions[i];
            var where = $"questions[{i}]";
            if (q.Kind is not ("mcq" or "case")) errors.Add($"{where}.kind must be \"mcq\" or \"case\"");
            if (q.Kind == "case" && string.IsNullOrWhiteSpace(q.Case)) errors.Add($"{where}.case is required for a case-based question");
            if (string.IsNullOrWhiteSpace(q.Text)) errors.Add($"{where}.text is required");
            if (q.Options.Count != 4) errors.Add($"{where} needs exactly 4 options");
            if (q.OptionsHi is { Count: > 0 } hi && hi.Count != 4) errors.Add($"{where}.optionsHi needs 4 options or none");
            if (q.Answer is < 0 or > 3) errors.Add($"{where}.answer must be 0–3");
            if (string.IsNullOrWhiteSpace(q.Explanation)) errors.Add($"{where}.explanation is required");
        }

        for (var i = 0; i < c.Faqs.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(c.Faqs[i].Q) || string.IsNullOrWhiteSpace(c.Faqs[i].A)) errors.Add($"faqs[{i}] needs q and a");
        }

        var ids = new HashSet<string>();
        for (var i = 0; i < c.Games.Count; i++)
        {
            errors.AddRange(ValidateGame(c.Games[i], $"games[{i}]", ids));
        }

        return errors;
    }

    private static IEnumerable<string> ValidateGame(JsonElement g, string where, HashSet<string> ids)
    {
        if (g.ValueKind != JsonValueKind.Object)
        {
            yield return $"{where} must be an object";
            yield break;
        }

        var type = Str(g, "type");
        var id = Str(g, "id");
        if (type is null || !GameTypes.Contains(type)) yield return $"{where}.type must be one of {string.Join(", ", GameTypes)}";
        if (id is null || !SlugPattern().IsMatch(id)) yield return $"{where}.id must be lowercase words joined by hyphens";
        else if (!ids.Add(id)) yield return $"{where}.id \"{id}\" is used twice";
        if (Str(g, "title") is null) yield return $"{where}.title is required";

        switch (type)
        {
            case "dragSort":
                var buckets = Arr(g, "buckets");
                if (buckets.Count is < 2 or > 4) yield return $"{where} needs 2–4 buckets";
                var bucketIds = buckets.Select(b => Str(b, "id")).ToHashSet();
                foreach (var item in Arr(g, "items"))
                {
                    if (!bucketIds.Contains(Str(item, "bucket"))) yield return $"{where}: item \"{Str(item, "text")}\" has an unknown bucket";
                }

                if (Arr(g, "items").Count < 2) yield return $"{where} needs at least 2 items";
                break;
            case "matchPairs":
                if (Arr(g, "pairs").Count < 2) yield return $"{where} needs at least 2 pairs";
                break;
            case "timeline":
                if (Arr(g, "items").Count < 3) yield return $"{where} needs at least 3 items";
                break;
            case "labelDiagram":
                if (Str(g, "image") is null) yield return $"{where}.image is required";
                foreach (var h in Arr(g, "hotspots"))
                {
                    if (Num(h, "x") is not (>= 0 and <= 100) || Num(h, "y") is not (>= 0 and <= 100)) yield return $"{where}: hotspot x and y must be 0–100 (%)";
                }

                if (Arr(g, "hotspots").Count < 2) yield return $"{where} needs at least 2 hotspots";
                break;
            case "numberLine":
                var min = Num(g, "min");
                var max = Num(g, "max");
                var step = Num(g, "step");
                if (min is null || max is null || step is null || min >= max || step <= 0) yield return $"{where} needs min < max and step > 0";
                foreach (var r in Arr(g, "rounds"))
                {
                    var v = Num(r, "value");
                    if (v is null || v < min || v > max) yield return $"{where}: every round's value must be between min and max";
                }

                if (Arr(g, "rounds").Count < 1) yield return $"{where} needs at least 1 round";
                break;
            case "speedQuiz":
                var questions = Arr(g, "questions");
                if (questions.Count < 5) yield return $"{where} needs at least 5 questions";
                foreach (var q in questions)
                {
                    if (Arr(q, "options").Count is < 2 or > 4) yield return $"{where}: each question needs 2–4 options";
                    if (Num(q, "answer") is not { } a || a < 0 || a >= Arr(q, "options").Count) yield return $"{where}: an answer index is out of range";
                }

                break;
        }
    }

    private static string? Str(JsonElement e, string name) =>
        e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(v.GetString()) ? v.GetString() : null;

    private static double? Num(JsonElement e, string name) =>
        e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : null;

    private static List<JsonElement> Arr(JsonElement e, string name) =>
        e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Array ? [.. v.EnumerateArray()] : [];
}
