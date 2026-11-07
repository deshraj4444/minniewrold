using MinnieWorld.Web.Services.Astrology;

namespace MinnieWorld.Tests;

/// <summary>Checks the astrology engine against published reference values.</summary>
public class EphemerisTests
{
    private static double UtFromTt(double jde) => jde - Ephemeris.DeltaTDays(jde);

    private static void AssertAngle(double expected, double actual, double toleranceDegrees)
    {
        var diff = Math.Abs(((actual - expected + 540) % 360) - 180);
        Assert.True(diff <= toleranceDegrees, $"expected {expected:F4}°, got {actual:F4}° (off by {diff * 60:F2}')");
    }

    [Fact]
    public void JulianDay_matches_Meeus_example_7a()
    {
        // 1957 October 4.81 → JD 2436116.31
        Assert.Equal(2436116.31, Ephemeris.JulianDay(new DateTime(1957, 10, 4, 19, 26, 24, DateTimeKind.Utc)), 2);
    }

    [Fact]
    public void Moon_longitude_matches_Meeus_example_47a()
    {
        // 1992 April 12, 0h TD: λ = 133.162655° (geometric, before nutation)
        AssertAngle(133.162655, Ephemeris.TropicalLongitude(Graha.Moon, UtFromTt(2448724.5)), 0.01);
    }

    [Fact]
    public void Sun_longitude_matches_Meeus_example_25a()
    {
        // 1992 October 13, 0h TD: apparent λ = 199.90988° (low-accuracy method)
        AssertAngle(199.90988, Ephemeris.TropicalLongitude(Graha.Sun, UtFromTt(2448908.5)), 0.01);
    }

    [Fact]
    public void Venus_longitude_matches_Meeus_example_33a()
    {
        // 1992 December 20, 0h TD: apparent geocentric λ = 313.08102°
        AssertAngle(313.08102, Ephemeris.TropicalLongitude(Graha.Venus, UtFromTt(2448976.5)), 0.05);
    }

    [Fact]
    public void Lahiri_ayanamsa_is_about_24_degrees_11_minutes_in_2024()
    {
        var jd = Ephemeris.JulianDay(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        Assert.InRange(Ephemeris.LahiriAyanamsa(jd), 24.18, 24.21);
    }

    [Fact]
    public void Ascendant_at_equator_with_0_Aries_culminating_is_0_Cancer()
    {
        // Find a moment when RAMC = 0 at longitude 0 and check the tropical ascendant (= sidereal + ayanamsa) is 90°.
        var jd = 2451545.0 - (280.46061837 / 360.98564736629);
        var asc = Ephemeris.SiderealAscendant(jd, 0, 0) + Ephemeris.LahiriAyanamsa(jd);
        AssertAngle(90, asc, 0.01);
    }
}

public class PanchangTests
{
    private const double DelhiLat = 28.6139, DelhiLon = 77.2090;

    [Fact]
    public void Sharad_Purnima_2024_has_purnima_ending_around_4_56_pm()
    {
        // Full moon: 2024-10-17 11:26 UTC = 16:56 IST.
        var p = PanchangCalculator.Calculate(new DateOnly(2024, 10, 17), DelhiLat, DelhiLon, "Asia/Kolkata", "Delhi");
        Assert.Equal("Purnima", p.Tithi.Name);
        Assert.Equal("Shukla Paksha", p.Paksha);
        Assert.NotNull(p.Tithi.EndsAt);
        Assert.InRange(p.Tithi.EndsAt!.Value, new DateTime(2024, 10, 17, 16, 50, 0), new DateTime(2024, 10, 17, 17, 2, 0));
        Assert.Equal("Ashwin", p.AmantaMonth);
        Assert.Equal("Revati", p.Nakshatra.Name);
    }

    [Fact]
    public void Diwali_2024_is_amavasya_in_amanta_ashwin_and_purnimanta_kartik()
    {
        // New moon: 2024-11-01 12:47 UTC = 18:17 IST.
        var p = PanchangCalculator.Calculate(new DateOnly(2024, 11, 1), DelhiLat, DelhiLon, "Asia/Kolkata", "Delhi");
        Assert.Equal("Amavasya", p.Tithi.Name);
        Assert.InRange(p.Tithi.EndsAt!.Value, new DateTime(2024, 11, 1, 18, 10, 0), new DateTime(2024, 11, 1, 18, 24, 0));
        Assert.Equal("Ashwin", p.AmantaMonth);
        Assert.Equal("Kartik", p.PurnimantaMonth);
        Assert.Equal(2081, p.VikramSamvat);
    }

    [Theory]
    [InlineData(2025, 6, 21, 5, 24, 19, 22)]
    [InlineData(2025, 12, 21, 7, 10, 17, 29)]
    public void Delhi_sunrise_and_sunset_are_within_three_minutes(int y, int m, int d, int riseH, int riseM, int setH, int setM)
    {
        var p = PanchangCalculator.Calculate(new DateOnly(y, m, d), DelhiLat, DelhiLon, "Asia/Kolkata", "Delhi");
        Assert.InRange((p.Sunrise - new DateTime(y, m, d, riseH, riseM, 0)).TotalMinutes, -3, 3);
        Assert.InRange((p.Sunset - new DateTime(y, m, d, setH, setM, 0)).TotalMinutes, -3, 3);
    }
}

public class KundaliTests
{
    private static readonly BirthDetails Sample = new("Test", new DateOnly(1990, 5, 15), new TimeOnly(10, 30), 28.6139, 77.2090, "Asia/Kolkata", "New Delhi");

    [Fact]
    public void Chart_has_lagna_nine_grahas_and_twelve_houses()
    {
        var chart = KundaliCalculator.Calculate(Sample, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        Assert.Equal(9, chart.Planets.Count);
        Assert.Equal(12, chart.LagnaChart.Count);
        Assert.Equal(1, chart.LagnaChart[0].House);
        Assert.Equal(chart.Lagna.Rashi, chart.LagnaChart[0].Rashi);
        Assert.Equal(9, chart.LagnaChart.Sum(h => h.Grahas.Count));
        var rahu = chart.Planets.Single(p => p.Graha == "Rahu");
        var ketu = chart.Planets.Single(p => p.Graha == "Ketu");
        Assert.Equal((rahu.Rashi + 6) % 12, ketu.Rashi);
    }

    [Fact]
    public void Sun_in_mid_May_is_in_sidereal_Vrishabha()
    {
        var chart = KundaliCalculator.Calculate(Sample, DateTime.UtcNow);
        Assert.Equal("Vrishabha", chart.Planets.Single(p => p.Graha == "Sun").RashiName);
    }

    [Fact]
    public void Vimshottari_covers_120_years_from_birth_and_antardashas_tile_each_mahadasha()
    {
        var chart = KundaliCalculator.Calculate(Sample, DateTime.UtcNow);
        Assert.Equal(9, chart.Dashas.Count);
        Assert.Equal(Sample.Date, chart.Dashas[0].Start);
        for (var i = 1; i < chart.Dashas.Count; i++)
        {
            Assert.Equal(chart.Dashas[i - 1].End, chart.Dashas[i].Start);
            Assert.Equal(chart.Dashas[i].Start, chart.Dashas[i].Antardashas![0].Start);
            Assert.Equal(chart.Dashas[i].End, chart.Dashas[i].Antardashas![^1].End);
        }

        var moonNak = Vedic.Nakshatras.Single(n => n.Name == chart.Planets.Single(p => p.Graha == "Moon").Nakshatra);
        Assert.Equal(moonNak.Lord.ToString(), chart.Dashas[0].Lord);
    }

    [Fact]
    public void Matching_same_person_scores_known_values()
    {
        var chart = KundaliCalculator.Calculate(Sample, DateTime.UtcNow);
        var result = Matching.Match(chart, chart);
        // Same nakshatra: same nadi (0/8), same rashi (bhakoot 7/7), same yoni (4/4), same gana (6/6), maitri 5/5.
        Assert.Equal(0, result.Kootas.Single(k => k.Name == "Nadi").Obtained);
        Assert.Equal(7, result.Kootas.Single(k => k.Name == "Bhakoot").Obtained);
        Assert.Equal(4, result.Kootas.Single(k => k.Name == "Yoni").Obtained);
        Assert.Equal(36, result.Kootas.Sum(k => k.Maximum));
    }
}

public class NumerologyTests
{
    [Fact]
    public void Moolank_and_bhagyank_from_date()
    {
        // 15/05/1990: moolank 1+5 = 6; bhagyank 1+5+0+5+1+9+9+0 = 30 → 3.
        var r = Numerology.Calculate("Raj", new DateOnly(1990, 5, 15));
        Assert.Equal(6, r.Moolank.Number);
        Assert.Equal(3, r.Bhagyank.Number);
        // R2 + A1 + J1 = 4
        Assert.Equal(4, r.Namank.Number);
    }
}
