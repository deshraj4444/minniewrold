using System.ComponentModel.DataAnnotations;

namespace MinnieWorld.Web.Contracts;

public record LoginRequest
{
    [Required, EmailAddress] public string Email { get; init; } = "";
    [Required] public string Password { get; init; } = "";
}

public record ContactRequest
{
    [Required, RegularExpression("^(astro|dharma|gk|hub)$")] public string SiteKey { get; init; } = "hub";
    [Required, StringLength(120)] public string FullName { get; init; } = "";
    [Required, EmailAddress, StringLength(256)] public string Email { get; init; } = "";
    [StringLength(30)] public string Phone { get; init; } = "";
    [StringLength(200)] public string Subject { get; init; } = "";
    [Required, StringLength(4000, MinimumLength = 5)] public string Message { get; init; } = "";
}

public record ConsultationRequestDto
{
    [Required, StringLength(120)] public string FullName { get; init; } = "";
    [Required, EmailAddress, StringLength(256)] public string Email { get; init; } = "";
    [Required, StringLength(30)] public string Phone { get; init; } = "";
    [Required] public DateOnly? BirthDate { get; init; }
    [Required] public TimeOnly? BirthTime { get; init; }
    [Required, StringLength(200)] public string BirthPlace { get; init; } = "";
    [Required, StringLength(100)] public string ServiceType { get; init; } = "";
    [StringLength(2000)] public string Notes { get; init; } = "";
}

public record ConsultationStatusRequest
{
    [Required] public string Status { get; init; } = "";
}

public record SiteRequest
{
    [Required, StringLength(100)] public string Name { get; init; } = "";
    [StringLength(300)] public string Tagline { get; init; } = "";
    [Required, StringLength(300)] public string Path { get; init; } = "";
    [StringLength(500)] public string ThumbnailUrl { get; init; } = "";
    [StringLength(20)] public string AccentColor { get; init; } = "#4f46e5";
    public int SortOrder { get; init; }
    public bool IsActive { get; init; } = true;
}

public record TestimonialRequest
{
    [Required, StringLength(120)] public string CustomerName { get; init; } = "";
    [StringLength(120)] public string Location { get; init; } = "";
    [Required, StringLength(1000)] public string Quote { get; init; } = "";
    [Range(1, 5)] public int Rating { get; init; } = 5;
    [StringLength(500)] public string AvatarUrl { get; init; } = "";
    public bool IsPublished { get; init; } = true;
}

public record CourseRequest
{
    [Required, RegularExpression("^(dharma|gk)$")] public string Blog { get; init; } = "";
    [Required, StringLength(200)] public string Title { get; init; } = "";
    /// <summary>Optional; generated from the title when blank.</summary>
    [StringLength(220)] public string Slug { get; init; } = "";
    [StringLength(400)] public string Subtitle { get; init; } = "";
    public string Description { get; init; } = "";
    [Required, StringLength(80)] public string Category { get; init; } = "";
    [StringLength(30)] public string Level { get; init; } = "Beginner";
    [StringLength(500)] public string ThumbnailUrl { get; init; } = "";
    public bool IsFeatured { get; init; }
    public bool IsPublished { get; init; } = true;
}

public record LessonRequest
{
    [Required, StringLength(200)] public string Title { get; init; } = "";
    [StringLength(220)] public string Slug { get; init; } = "";
    [StringLength(500)] public string Summary { get; init; } = "";
    public string Content { get; init; } = "";
    [StringLength(500)] public string VideoUrl { get; init; } = "";
    [Range(0, 1000)] public int DurationMinutes { get; init; }
    public int? SortOrder { get; init; }
    public bool IsPublished { get; init; } = true;
}

public record PostRequest
{
    [Required, RegularExpression("^(dharma|gk)$")] public string Blog { get; init; } = "";
    [Required, StringLength(200)] public string Title { get; init; } = "";
    [StringLength(220)] public string Slug { get; init; } = "";
    [StringLength(500)] public string Excerpt { get; init; } = "";
    public string Content { get; init; } = "";
    [Required, StringLength(80)] public string Category { get; init; } = "";
    [StringLength(300)] public string Tags { get; init; } = "";
    [StringLength(500)] public string CoverImageUrl { get; init; } = "";
    [StringLength(120)] public string Author { get; init; } = "";
    public bool IsPublished { get; init; } = true;
}

/// <summary>Birth details for chart calculations. Place comes from /api/astro/places (lat, lon and IANA time zone).</summary>
public record BirthInput
{
    [Required, StringLength(80)] public string Name { get; init; } = "";
    [Required] public DateOnly? Date { get; init; }
    [Required] public TimeOnly? Time { get; init; }
    [Range(-66.5, 66.5, ErrorMessage = "Charts above the polar circles are not supported.")] public double Latitude { get; init; }
    [Range(-180, 180)] public double Longitude { get; init; }
    [Required, StringLength(64)] public string TimeZone { get; init; } = "";
    [StringLength(200)] public string Place { get; init; } = "";
}

public record MatchRequest
{
    [Required] public BirthInput? Boy { get; init; }
    [Required] public BirthInput? Girl { get; init; }
}

public record VarshphalRequest
{
    [Required] public BirthInput? Birth { get; init; }
    [Range(1800, 2200)] public int Year { get; init; }
}

public record StudentRegisterRequest
{
    [Required, StringLength(120, MinimumLength = 2)] public string FullName { get; init; } = "";
    [Required, EmailAddress, StringLength(256)] public string Email { get; init; } = "";
    [StringLength(30)] public string Phone { get; init; } = "";
    [Required, StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")] public string Password { get; init; } = "";
}

public record SaveAnswersRequest
{
    /// <summary>Question id → chosen option (0–3). A missing or null entry means unanswered.</summary>
    [Required] public Dictionary<int, int?>? Answers { get; init; }
}

public record ExamRequest
{
    [Required, StringLength(200)] public string Title { get; init; } = "";
    [StringLength(220)] public string Slug { get; init; } = "";
    [StringLength(2000)] public string Description { get; init; } = "";
    [Required, StringLength(100)] public string Category { get; init; } = "";
    [Required, RegularExpression("^(Mock test|Previous year|Sectional|Sample test|Year pattern|Expected paper)$")] public string Kind { get; init; } = "Mock test";
    [Range(1900, 2200)] public int? Year { get; init; }
    [Range(1, 600)] public int DurationMinutes { get; init; } = 60;
    [Range(0, 5)] public decimal NegativeMark { get; init; }
    [Range(0, 10000)] public int Price { get; init; }
    public bool IsPublished { get; init; } = true;
}

public record PurchaseRequest
{
    [Required, RegularExpression(@"^\s*\d{12}\s*$", ErrorMessage = "Enter the 12-digit UPI transaction ID from your payment app.")] public string UpiRef { get; init; } = "";
}

public record AdminStudentRequest
{
    [Required, StringLength(120, MinimumLength = 2)] public string FullName { get; init; } = "";
    [Required, EmailAddress, StringLength(256)] public string Email { get; init; } = "";
    [StringLength(30)] public string Phone { get; init; } = "";
    /// <summary>Required when creating; leave empty when editing to keep the current password.</summary>
    [StringLength(128)] public string Password { get; init; } = "";
    public bool HasAllAccess { get; init; }
    public DateTime? AllAccessUntil { get; init; }
    public bool IsDisabled { get; init; }
}

public record GrantExamRequest
{
    [Range(1, int.MaxValue)] public int ExamId { get; init; }
}

public record RejectPaymentRequest
{
    [StringLength(500)] public string Note { get; init; } = "";
}

public record QuestionRequest
{
    [StringLength(100)] public string Section { get; init; } = "";
    [Required, StringLength(8000)] public string Text { get; init; } = "";
    [Required, MinLength(4), MaxLength(4)] public string[] Options { get; init; } = [];
    [Range(0, 3)] public int Answer { get; init; }
    [StringLength(2000)] public string Explanation { get; init; } = "";
    [StringLength(8000)] public string TextHi { get; init; } = "";
    /// <summary>Hindi options; empty or all-blank when the question has no Hindi version.</summary>
    [MaxLength(4)] public string[] OptionsHi { get; init; } = [];
    [StringLength(2000)] public string ExplanationHi { get; init; } = "";
    // Left unchanged when null, so an older admin form that doesn't send them can't wipe them.
    [Range(0.25, 10)] public decimal? Marks { get; init; }
    [StringLength(20000)] public string? Figure { get; init; }
    [StringLength(8000)] public string? Passage { get; init; }
    [StringLength(8000)] public string? PassageHi { get; init; }
}

public record ImportQuestionsRequest
{
    /// <summary>A JSON array of questions, or an object with a "questions" array. See ExamImport for the accepted field names.</summary>
    [Required] public string Json { get; init; } = "";
    /// <summary>When true the exam's existing questions are deleted first.</summary>
    public bool Replace { get; init; }
}

public record SettingsRequest
{
    [Required] public Dictionary<string, string>? Values { get; init; }
}
