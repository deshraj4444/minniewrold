using System.Text.Json;
using AstroPortal.Api.Contracts;
using AstroPortal.Api.Data;
using AstroPortal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AstroPortal.Api.Services;

public class FreeHoroscopeService : IHoroscopeService
{
    private static readonly string[] ValidSigns =
    {
        "aries", "taurus", "gemini", "cancer", "leo", "virgo",
        "libra", "scorpio", "sagittarius", "capricorn", "aquarius", "pisces"
    };

    private readonly AstroDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FreeHoroscopeService> _logger;

    public FreeHoroscopeService(
        AstroDbContext dbContext,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<FreeHoroscopeService> logger)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HoroscopeResponse> GetDailyHoroscopeAsync(string sign, CancellationToken cancellationToken)
    {
        var normalizedSign = NormalizeSign(sign);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var cached = await _dbContext.HoroscopeCaches
            .AsNoTracking()
            .FirstOrDefaultAsync(cache => cache.Sign == normalizedSign && cache.HoroscopeDate == today, cancellationToken);

        if (cached is not null)
        {
            return new HoroscopeResponse(ToTitleCase(cached.Sign), cached.HoroscopeDate, cached.Content, cached.Source);
        }

        var horoscope = await TryFetchFromFreeApiAsync(normalizedSign, today, cancellationToken)
            ?? BuildFallbackHoroscope(normalizedSign, today);

        _dbContext.HoroscopeCaches.Add(new HoroscopeCache
        {
            Sign = normalizedSign,
            HoroscopeDate = horoscope.Date,
            Content = horoscope.Content,
            Source = horoscope.Source,
            FetchedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return horoscope;
    }

    private async Task<HoroscopeResponse?> TryFetchFromFreeApiAsync(string sign, DateOnly today, CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["FreeHoroscopeApi:BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        try
        {
            var separator = baseUrl.Contains('?') ? '&' : '?';
            using var response = await _httpClient.GetAsync($"{baseUrl}{separator}sign={sign}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Free horoscope API returned {StatusCode} for {Sign}", response.StatusCode, sign);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var content = ExtractHoroscopeText(document.RootElement);

            return string.IsNullOrWhiteSpace(content)
                ? null
                : new HoroscopeResponse(ToTitleCase(sign), today, content, "FreeHoroscopeAPI");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "Unable to fetch horoscope for {Sign}; using local fallback", sign);
            return null;
        }
    }

    private static string ExtractHoroscopeText(JsonElement root)
    {
        if (root.TryGetProperty("data", out var data))
        {
            if (data.TryGetProperty("horoscope_data", out var horoscopeData))
            {
                return horoscopeData.GetString() ?? string.Empty;
            }

            if (data.TryGetProperty("prediction", out var prediction))
            {
                return prediction.GetString() ?? string.Empty;
            }
        }

        if (root.TryGetProperty("horoscope", out var horoscope))
        {
            return horoscope.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static HoroscopeResponse BuildFallbackHoroscope(string sign, DateOnly today)
    {
        var content = $"Today invites {ToTitleCase(sign)} to pause, prioritize calm decisions, and choose conversations that create clarity. Focus on one meaningful action, protect your energy, and let intuition guide the next step.";
        return new HoroscopeResponse(ToTitleCase(sign), today, content, "Local fallback");
    }

    private static string NormalizeSign(string sign)
    {
        var normalized = sign.Trim().ToLowerInvariant();

        if (!ValidSigns.Contains(normalized))
        {
            throw new ArgumentException("Invalid zodiac sign.", nameof(sign));
        }

        return normalized;
    }

    private static string ToTitleCase(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}
