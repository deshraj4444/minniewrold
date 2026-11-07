namespace MinnieWorld.Web.Models;

/// <summary>The blogs hosted on this site. Each has its own URL prefix, look and categories.</summary>
public static class BlogKeys
{
    public const string Dharma = "dharma";
    public const string Gk = "gk";

    public static readonly string[] All = [Dharma, Gk];

    public static bool IsValid(string? key) => key is not null && All.Contains(key);
}

public class Course
{
    public int Id { get; set; }
    /// <summary>One of <see cref="BlogKeys"/>.</summary>
    public string Blog { get; set; } = BlogKeys.Gk;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    /// <summary>Markdown.</summary>
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Level { get; set; } = "Beginner";
    public string ThumbnailUrl { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<Lesson> Lessons { get; set; } = [];
}

public class Lesson
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    /// <summary>Markdown.</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>Optional YouTube/Vimeo link, embedded above the lesson text.</summary>
    public string VideoUrl { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Post
{
    public int Id { get; set; }
    /// <summary>One of <see cref="BlogKeys"/>.</summary>
    public string Blog { get; set; } = BlogKeys.Gk;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    /// <summary>Markdown.</summary>
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    /// <summary>Comma-separated.</summary>
    public string Tags { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = true;
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
