using System.Text.Json;

namespace MinnieWorld.Web.Services.Astrology;

public record Place(string Name, string Region, string Country, double Latitude, double Longitude, string TimeZone);

/// <summary>Birth-place search through Open-Meteo's free geocoding API (no key), which also returns the IANA time zone.</summary>
public class PlaceService(HttpClient http, ILogger<PlaceService> logger)
{
    public async Task<IReadOnlyList<Place>> SearchAsync(string query, CancellationToken ct)
    {
        if (query.Trim().Length < 2)
        {
            return [];
        }

        try
        {
            var url = $"https://geocoding-api.open-meteo.com/v1/search?count=8&language=en&format=json&name={Uri.EscapeDataString(query.Trim())}";
            using var response = await http.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            if (!doc.RootElement.TryGetProperty("results", out var results))
            {
                return [];
            }

            return results.EnumerateArray()
                .Where(r => r.TryGetProperty("timezone", out _))
                .Select(r => new Place(
                    r.GetProperty("name").GetString() ?? "",
                    r.TryGetProperty("admin1", out var a) ? a.GetString() ?? "" : "",
                    r.TryGetProperty("country", out var c) ? c.GetString() ?? "" : "",
                    Math.Round(r.GetProperty("latitude").GetDouble(), 4),
                    Math.Round(r.GetProperty("longitude").GetDouble(), 4),
                    r.GetProperty("timezone").GetString() ?? "UTC"))
                .ToList();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Place search failed for {Query}", query);
            return [];
        }
    }

    /// <summary>Converts a local wall-clock birth time at <paramref name="timeZoneId"/> (IANA or Windows id) to UTC.</summary>
    public static DateTime ToUtc(DateOnly date, TimeOnly time, string timeZoneId)
    {
        var local = date.ToDateTime(time, DateTimeKind.Unspecified);
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        // A wall-clock time skipped by a DST change has no UTC equivalent; nudge it forward an hour.
        if (tz.IsInvalidTime(local)) local = local.AddHours(1);
        return TimeZoneInfo.ConvertTimeToUtc(local, tz);
    }

    public static bool IsKnownTimeZone(string id) => TimeZoneInfo.TryFindSystemTimeZoneById(id, out _);
}
