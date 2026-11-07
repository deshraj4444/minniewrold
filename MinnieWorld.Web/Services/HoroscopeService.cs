using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MinnieWorld.Web.Data;
using MinnieWorld.Web.Models;

namespace MinnieWorld.Web.Services;

public record HoroscopeResponse(string Sign, string Period, DateOnly Date, string Content, string Source);

/// <summary>Horoscope by sign and period: served from the DB cache, else fetched from the free API, else a local fallback.</summary>
public class HoroscopeService(AppDbContext db, HttpClient http, IConfiguration config, ILogger<HoroscopeService> logger)
{
    public static readonly string[] Signs =
    [
        "aries", "taurus", "gemini", "cancer", "leo", "virgo",
        "libra", "scorpio", "sagittarius", "capricorn", "aquarius", "pisces"
    ];

    public static readonly string[] Periods = ["daily", "weekly", "monthly"];

    public static bool IsValidSign(string sign) => Signs.Contains(sign.Trim().ToLowerInvariant());

    public async Task<HoroscopeResponse> GetAsync(string sign, string period, CancellationToken ct)
    {
        var normalized = sign.Trim().ToLowerInvariant();
        var start = PeriodStart(period, DateOnly.FromDateTime(DateTime.UtcNow));

        var cached = await db.HoroscopeCaches.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Sign == normalized && c.Period == period && c.HoroscopeDate == start, ct);
        if (cached is not null)
        {
            return new HoroscopeResponse(Title(cached.Sign), period, cached.HoroscopeDate, cached.Content, cached.Source);
        }

        var text = await TryFetchAsync(normalized, period, ct);
        if (text is null)
        {
            // Don't cache the fallback, so the next request retries the API.
            return new HoroscopeResponse(Title(normalized), period, start, Fallback(normalized, period), "AstroAura");
        }

        db.HoroscopeCaches.Add(new HoroscopeCache { Sign = normalized, Period = period, HoroscopeDate = start, Content = text, Source = "Horoscope API" });
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Another request cached the same sign/period first; ours is equivalent.
        }

        return new HoroscopeResponse(Title(normalized), period, start, text, "Horoscope API");
    }

    private static DateOnly PeriodStart(string period, DateOnly today) => period switch
    {
        "weekly" => today.AddDays(-(((int)today.DayOfWeek + 6) % 7)),
        "monthly" => new DateOnly(today.Year, today.Month, 1),
        _ => today
    };

    private async Task<string?> TryFetchAsync(string sign, string period, CancellationToken ct)
    {
        var baseUrl = config["HoroscopeApi:BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        try
        {
            var url = $"{baseUrl}/{period}?sign={Title(sign)}" + (period == "daily" ? "&day=TODAY" : "");
            using var response = await http.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Horoscope API returned {StatusCode} for {Sign}/{Period}", response.StatusCode, sign, period);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var text = Extract(doc.RootElement);
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Unable to fetch {Period} horoscope for {Sign}; using fallback", period, sign);
            return null;
        }
    }

    private static string Extract(JsonElement root)
    {
        if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
        {
            foreach (var name in new[] { "horoscope_data", "horoscope", "prediction" })
            {
                if (data.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString() ?? "";
                }
            }
        }

        return "";
    }

    private static string Fallback(string sign, string period)
    {
        var span = period switch { "weekly" => "This week", "monthly" => "This month", _ => "Today" };
        return $"{span} invites {Title(sign)} to pause, prioritise calm decisions and choose conversations that create clarity. " +
               "Focus on one meaningful action, protect your energy, and let intuition guide the next step.";
    }

    private static string Title(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}
