namespace AstroPortal.Api.Contracts;

public record BlogPostRequest(
    string Title,
    string Excerpt,
    string Content,
    string Category,
    string ImageUrl,
    bool IsPublished);

public record ContactRequestDto(
    string FullName,
    string Email,
    string Phone,
    string Subject,
    string Message);

public record KundaliRequestDto(
    string FullName,
    string Email,
    string Phone,
    DateOnly BirthDate,
    TimeOnly BirthTime,
    string BirthPlace,
    string ServiceType,
    string Notes);
