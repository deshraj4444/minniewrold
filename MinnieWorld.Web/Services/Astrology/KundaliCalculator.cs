namespace MinnieWorld.Web.Services.Astrology;

public record BirthDetails(string Name, DateOnly Date, TimeOnly Time, double Latitude, double Longitude, string TimeZone, string Place);

public record PlanetPosition(
    string Graha, string Name, string Abbr, double Longitude, string Degree,
    int Rashi, string RashiName, int House, string Nakshatra, int Pada, string NakshatraLord,
    bool Retrograde, string Dignity);

public record DashaPeriod(string Lord, DateOnly Start, DateOnly End, List<DashaPeriod>? Antardashas = null);

public record ManglikResult(bool FromLagna, bool FromMoon, int MarsHouseFromLagna, int MarsHouseFromMoon, string Summary);

public record AvakhadaDetails(
    string Lagna, string LagnaLord, string Rashi, string RashiLord, string Nakshatra, int Pada, string NakshatraLord,
    string Varna, string Vashya, string Yoni, string Gana, string Nadi, string SunSign, string Tithi, string Yoga, string Karana, string Weekday);

public record KundaliChart(
    BirthDetails Birth, DateTime BirthUtc, double Ayanamsa,
    PlanetPosition Lagna, List<PlanetPosition> Planets,
    /// <summary>12 entries: houses 1–12, each with its rashi index and the graha abbreviations in it (Lagna chart).</summary>
    List<ChartHouse> LagnaChart,
    List<ChartHouse> MoonChart,
    List<ChartHouse> NavamsaChart,
    AvakhadaDetails Avakhada,
    ManglikResult Manglik,
    List<DashaPeriod> Dashas,
    DashaPeriod? CurrentMahadasha,
    DashaPeriod? CurrentAntardasha,
    KundaliInsights Insights);

public record ChartHouse(int House, int Rashi, List<string> Grahas);

public static class KundaliCalculator
{
    private static readonly Graha[] AllGrahas = Enum.GetValues<Graha>();

    public static KundaliChart Calculate(BirthDetails birth, DateTime nowUtc)
    {
        var utc = PlaceService.ToUtc(birth.Date, birth.Time, birth.TimeZone);
        var jd = Ephemeris.JulianDay(utc);

        var ascLon = Ephemeris.SiderealAscendant(jd, birth.Latitude, birth.Longitude);
        var lagnaRashi = Vedic.RashiOf(ascLon);

        var planets = AllGrahas.Select(g =>
        {
            var lon = Ephemeris.SiderealLongitude(g, jd);
            // Rahu and Ketu always move backwards (mean node), which is how panchangs print them.
            var retro = g is Graha.Rahu or Graha.Ketu || (g is not (Graha.Sun or Graha.Moon) && Ephemeris.Speed(g, jd) < 0);
            return Position(g.ToString(), Vedic.GrahaNames[g], Vedic.GrahaAbbr[g], lon, lagnaRashi, retro, Vedic.Dignity(g, Vedic.RashiOf(lon)));
        }).ToList();

        var lagna = Position("Lagna", "Lagna", "As", ascLon, lagnaRashi, false, "");
        var moon = planets.First(p => p.Graha == nameof(Graha.Moon));
        var sun = planets.First(p => p.Graha == nameof(Graha.Sun));

        var dashas = Vimshottari(Ephemeris.SiderealLongitude(Graha.Moon, jd), DateOnly.FromDateTime(utc));
        var today = DateOnly.FromDateTime(nowUtc);
        var currentMd = dashas.FirstOrDefault(d => d.Start <= today && today < d.End);
        var currentAd = currentMd?.Antardashas?.FirstOrDefault(d => d.Start <= today && today < d.End);

        var panchang = PanchangCalculator.ElementsAt(jd);

        return new KundaliChart(
            birth, utc, Math.Round(Ephemeris.LahiriAyanamsa(jd), 4),
            lagna, planets,
            BuildChart(lagnaRashi, planets, p => p.Rashi),
            BuildChart(moon.Rashi, planets, p => p.Rashi),
            BuildChart(Navamsa(ascLon), planets, p => Navamsa(p.Longitude)),
            Avakhada(lagna, moon, sun, panchang, utc, birth.TimeZone),
            Manglik(planets, lagnaRashi, moon.Rashi),
            dashas, currentMd, currentAd,
            KundaliInsightsCalculator.Calculate(planets, lagna, utc, nowUtc));
    }

    internal static PlanetPosition Position(string key, string name, string abbr, double lon, int lagnaRashi, bool retro, string dignity)
    {
        var rashi = Vedic.RashiOf(lon);
        var nak = Vedic.Nakshatras[Vedic.NakshatraOf(lon)];
        return new PlanetPosition(
            key, name, abbr, Math.Round(lon, 4), Vedic.Dms(lon % 30),
            rashi, Vedic.Rashis[rashi].Name, ((rashi - lagnaRashi + 12) % 12) + 1,
            nak.Name, Vedic.PadaOf(lon), nak.Lord.ToString(), retro, dignity);
    }

    /// <summary>Whole-sign houses: house N holds the rashi N−1 signs after the first house's rashi.</summary>
    internal static List<ChartHouse> BuildChart(int firstRashi, List<PlanetPosition> planets, Func<PlanetPosition, int> rashiOf) =>
        Enumerable.Range(0, 12).Select(i =>
        {
            var rashi = (firstRashi + i) % 12;
            return new ChartHouse(i + 1, rashi, planets.Where(p => rashiOf(p) == rashi).Select(p => p.Abbr + (p.Retrograde && p.Graha is not ("Rahu" or "Ketu") ? "(R)" : "")).ToList());
        }).ToList();

    /// <summary>D9: each sign splits into nine 3°20' parts, counted from Aries/Capricorn/Libra/Cancer for fire/earth/air/water signs.</summary>
    private static int Navamsa(double lon)
    {
        var rashi = Vedic.RashiOf(lon);
        var part = (int)(lon % 30 / (30.0 / 9));
        var start = (rashi % 4) switch { 0 => 0, 1 => 9, 2 => 6, _ => 3 };
        return (start + part) % 12;
    }

    private static ManglikResult Manglik(List<PlanetPosition> planets, int lagnaRashi, int moonRashi)
    {
        int[] doshaHouses = [1, 2, 4, 7, 8, 12];
        var mars = planets.First(p => p.Graha == nameof(Graha.Mars));
        var fromLagna = ((mars.Rashi - lagnaRashi + 12) % 12) + 1;
        var fromMoon = ((mars.Rashi - moonRashi + 12) % 12) + 1;
        var l = doshaHouses.Contains(fromLagna);
        var m = doshaHouses.Contains(fromMoon);
        var summary = (l, m) switch
        {
            (true, true) => "Manglik: Mars is in a dosha house from both the Lagna and the Moon.",
            (true, false) => "Manglik from the Lagna (Mars in house " + fromLagna + "); not from the Moon. Often considered a mild dosha.",
            (false, true) => "Manglik from the Moon (Mars in house " + fromMoon + "); not from the Lagna. Often considered a mild dosha.",
            _ => "Not Manglik: Mars is not in houses 1, 2, 4, 7, 8 or 12 from the Lagna or the Moon."
        };
        return new ManglikResult(l, m, fromLagna, fromMoon, summary);
    }

    public static List<DashaPeriod> Vimshottari(double moonLongitude, DateOnly birthDate)
    {
        const double daysPerYear = 365.2425;
        var nakIndex = Vedic.NakshatraOf(moonLongitude);
        var traversed = Ephemeris.Norm(moonLongitude) % Vedic.NakshatraSpan / Vedic.NakshatraSpan;
        var startIdx = nakIndex % 9;

        var result = new List<DashaPeriod>();
        var birth = birthDate.ToDateTime(TimeOnly.MinValue);
        // The first mahadasha started before birth; place it so only the untraversed part remains after birth.
        var cursor = birth.AddDays(-traversed * Vedic.Vimshottari[startIdx].Years * daysPerYear);

        for (var i = 0; i < 9; i++)
        {
            var (lord, years) = Vedic.Vimshottari[(startIdx + i) % 9];
            var mdStart = cursor;
            var mdEnd = mdStart.AddDays(years * daysPerYear);

            var ads = new List<DashaPeriod>();
            var adCursor = mdStart;
            for (var j = 0; j < 9; j++)
            {
                var (adLord, adYears) = Vedic.Vimshottari[(startIdx + i + j) % 9];
                var adEnd = adCursor.AddDays(years * adYears / 120.0 * daysPerYear);
                if (adEnd > birth)
                {
                    ads.Add(new DashaPeriod(adLord.ToString(), DateOnly.FromDateTime(adCursor < birth ? birth : adCursor), DateOnly.FromDateTime(adEnd)));
                }

                adCursor = adEnd;
            }

            result.Add(new DashaPeriod(lord.ToString(), DateOnly.FromDateTime(mdStart < birth ? birth : mdStart), DateOnly.FromDateTime(mdEnd), ads));
            cursor = mdEnd;
        }

        return result;
    }

    private static AvakhadaDetails Avakhada(PlanetPosition lagna, PlanetPosition moon, PlanetPosition sun, PanchangElements p, DateTime utc, string tz)
    {
        var nak = Vedic.Nakshatras.First(n => n.Name == moon.Nakshatra);
        var localDay = TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById(tz)).DayOfWeek;
        return new AvakhadaDetails(
            lagna.RashiName, Vedic.Rashis[lagna.Rashi].Lord.ToString(),
            moon.RashiName, Vedic.Rashis[moon.Rashi].Lord.ToString(),
            moon.Nakshatra, moon.Pada, nak.Lord.ToString(),
            Matching.Varna(moon.Rashi), Matching.Vashya(moon.Longitude),
            nak.Yoni, nak.Gana, nak.Nadi,
            sun.RashiName, p.Tithi, p.Yoga, p.Karana, PanchangCalculator.Vara[(int)localDay]);
    }
}
