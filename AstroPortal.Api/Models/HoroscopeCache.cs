namespace AstroPortal.Api.Models;

public class HoroscopeCache
{
    public int Id { get; set; }
    public string Sign { get; set; } = string.Empty;
    public DateOnly HoroscopeDate { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}
