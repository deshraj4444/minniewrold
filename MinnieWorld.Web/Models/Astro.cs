namespace MinnieWorld.Web.Models;

public class HoroscopeCache
{
    public int Id { get; set; }
    public string Sign { get; set; } = string.Empty;
    /// <summary>daily, weekly or monthly.</summary>
    public string Period { get; set; } = "daily";
    /// <summary>Start of the period: the day, the Monday of the week, or the 1st of the month.</summary>
    public DateOnly HoroscopeDate { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>A request for a paid/personal reading by an astrologer (kundali reading, matching, career, etc.).</summary>
public class ConsultationRequest
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public TimeOnly BirthTime { get; set; }
    public string BirthPlace { get; set; } = string.Empty;
    public string ServiceType { get; set; } = "Kundali Consultation";
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = ConsultationStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class ConsultationStatus
{
    public const string New = "New";
    public const string InProgress = "In progress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = [New, InProgress, Completed, Cancelled];
}

public class Testimonial
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Quote { get; set; } = string.Empty;
    public int Rating { get; set; } = 5;
    public string AvatarUrl { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = true;
}
