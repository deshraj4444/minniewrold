namespace MinnieWorld.Web.Services.Astrology;

public record MunthaInfo(int Rashi, string RashiName, int House, string Lord, bool Favourable, string Effect);

public record OfficeBearer(string Role, string Graha, string Dignity, int Strength, bool AspectsLagna);

public record MuddaDasha(string Lord, DateTime Start, DateTime End, int Days);

public record VarshphalChart(
    BirthDetails Birth, int Year, int Age,
    /// <summary>Moment the Sun returns to its natal sidereal longitude, local to the birth time zone.</summary>
    DateTime VarshaPravesh, DateTime VarshaPraveshUtc, string Weekday, bool DayChart,
    PlanetPosition Lagna, List<PlanetPosition> Planets, List<ChartHouse> Chart,
    MunthaInfo Muntha, List<OfficeBearer> OfficeBearers, string YearLord, string YearLordReason,
    string PunyaSaham, List<MuddaDasha> MuddaDashas, DateTime NextVarshaPravesh);

/// <summary>
/// Tajik Varshphal (annual horoscope): the chart cast for the moment the Sun returns to its birth position each year,
/// at the birth place. Includes Muntha, the five office-bearers (Panchadhikari), the year lord and Mudda dasha.
/// </summary>
public static class VarshphalCalculator
{
    // Tri-rashi lords by Varsha Lagna sign (Aries first), for day and night charts (Tajik Neelakanthi).
    private static readonly Graha[] TriRashiDay =
        [Graha.Sun, Graha.Venus, Graha.Saturn, Graha.Venus, Graha.Jupiter, Graha.Moon, Graha.Mercury, Graha.Mars, Graha.Saturn, Graha.Mars, Graha.Jupiter, Graha.Moon];

    private static readonly Graha[] TriRashiNight =
        [Graha.Jupiter, Graha.Moon, Graha.Mercury, Graha.Mars, Graha.Sun, Graha.Venus, Graha.Saturn, Graha.Venus, Graha.Saturn, Graha.Mars, Graha.Mercury, Graha.Mars];

    private static readonly string[] MunthaEffects =
    [
        "Muntha in the Lagna: a year of personal progress, good health and recognition through your own efforts.",
        "Muntha in the 2nd: gains in wealth and family happiness; speech and savings improve.",
        "Muntha in the 3rd: courage and initiative bring success; good for short travel and help from siblings.",
        "Muntha in the 4th: worries about home, property or mother's health; avoid hasty property decisions.",
        "Muntha in the 5th: favourable for children, studies, creativity and wise investments.",
        "Muntha in the 6th: watch health, debts and disputes; competitors may be active.",
        "Muntha in the 7th: tensions in partnership or marriage and tiring travel; be patient with others.",
        "Muntha in the 8th: an obstacle-prone year; take care of health and avoid risks.",
        "Muntha in the 9th: fortune favours you; religious work, long journeys and support from elders.",
        "Muntha in the 10th: rise in career and status; recognition from authorities.",
        "Muntha in the 11th: gains, fulfilment of wishes and help from friends.",
        "Muntha in the 12th: expenses and losses are likely; spend carefully and look after sleep and health."
    ];

    private static readonly bool[] MunthaGood = [true, true, true, false, true, false, false, false, true, true, true, false];

    public static VarshphalChart Calculate(BirthDetails birth, int year)
    {
        var birthUtc = PlaceService.ToUtc(birth.Date, birth.Time, birth.TimeZone);
        var birthJd = Ephemeris.JulianDay(birthUtc);
        var natalSun = Ephemeris.SiderealLongitude(Graha.Sun, birthJd);
        var natalMoon = Ephemeris.SiderealLongitude(Graha.Moon, birthJd);
        var natalLagnaRashi = Vedic.RashiOf(Ephemeris.SiderealAscendant(birthJd, birth.Latitude, birth.Longitude));
        var age = year - birth.Date.Year;

        var jd = SolarReturn(natalSun, birthJd + (age * 365.2564));
        var nextJd = SolarReturn(natalSun, jd + 365.2564);
        var utc = Ephemeris.FromJulianDay(jd);
        var tz = TimeZoneInfo.FindSystemTimeZoneById(birth.TimeZone);
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, tz);

        var ascLon = Ephemeris.SiderealAscendant(jd, birth.Latitude, birth.Longitude);
        var lagnaRashi = Vedic.RashiOf(ascLon);
        var planets = Enum.GetValues<Graha>().Select(g =>
        {
            var lon = Ephemeris.SiderealLongitude(g, jd);
            var retro = g is Graha.Rahu or Graha.Ketu || (g is not (Graha.Sun or Graha.Moon) && Ephemeris.Speed(g, jd) < 0);
            return KundaliCalculator.Position(g.ToString(), Vedic.GrahaNames[g], Vedic.GrahaAbbr[g], lon, lagnaRashi, retro, Vedic.Dignity(g, Vedic.RashiOf(lon)));
        }).ToList();
        var lagna = KundaliCalculator.Position("Lagna", "Lagna", "As", ascLon, lagnaRashi, false, "");

        PlanetPosition P(Graha g) => planets.First(p => p.Graha == g.ToString());
        var sun = P(Graha.Sun);
        var moon = P(Graha.Moon);
        // The Sun is above the horizon when it lies in the half of the zodiac from the descendant forward to the ascendant.
        var dayChart = Ephemeris.Norm(sun.Longitude - ascLon) >= 180;

        var munthaRashi = (natalLagnaRashi + age) % 12;
        var munthaHouse = ((munthaRashi - lagnaRashi + 12) % 12) + 1;
        var munthaLord = Vedic.Rashis[munthaRashi].Lord;
        var muntha = new MunthaInfo(munthaRashi, Vedic.Rashis[munthaRashi].Name, munthaHouse, munthaLord.ToString(),
            MunthaGood[munthaHouse - 1], MunthaEffects[munthaHouse - 1]);

        var bearers = new (string Role, Graha Graha)[]
        {
            ("Muntha lord (Munthesh)", munthaLord),
            ("Birth Lagna lord (Janma Lagnesh)", Vedic.Rashis[natalLagnaRashi].Lord),
            ("Varsha Lagna lord (Varsha Lagnesh)", Vedic.Rashis[lagnaRashi].Lord),
            ("Tri-rashi lord (Tri-rashi pati)", (dayChart ? TriRashiDay : TriRashiNight)[lagnaRashi]),
            (dayChart ? "Day lord: Sun's sign lord (Dina pati)" : "Night lord: Moon's sign lord (Ratri pati)", Vedic.Rashis[(dayChart ? sun : moon).Rashi].Lord)
        }.Select(b =>
        {
            var p = P(b.Graha);
            return new OfficeBearer(b.Role, b.Graha.ToString(), p.Dignity, Strength(p.Dignity), Aspects(p.Rashi, lagnaRashi));
        }).ToList();

        var aspecting = bearers.Where(b => b.AspectsLagna).ToList();
        OfficeBearer lord;
        string reason;
        if (aspecting.Count > 0)
        {
            lord = aspecting.OrderByDescending(b => b.Strength).First();
            reason = $"{lord.Graha} is the strongest of the office-bearers that aspect the Varsha Lagna (by sign dignity: {lord.Dignity.ToLowerInvariant()}).";
        }
        else
        {
            lord = bearers[4];
            reason = $"None of the office-bearers aspects the Varsha Lagna, so the {(dayChart ? "day" : "night")} lord {lord.Graha} rules the year.";
        }

        // Punya Saham: Moon − Sun + Lagna by day, Sun − Moon + Lagna by night.
        var punya = Ephemeris.Norm(dayChart ? moon.Longitude - sun.Longitude + ascLon : sun.Longitude - moon.Longitude + ascLon);

        return new VarshphalChart(
            birth, year, age, local, utc, PanchangCalculator.Vara[(int)local.DayOfWeek], dayChart,
            lagna, planets, KundaliCalculator.BuildChart(lagnaRashi, planets, p => p.Rashi),
            muntha, bearers, lord.Graha, reason,
            $"{Vedic.Rashis[Vedic.RashiOf(punya)].Name} {Vedic.Dms(punya % 30)}",
            MuddaDashas(natalMoon, age, utc, Ephemeris.FromJulianDay(nextJd), tz),
            TimeZoneInfo.ConvertTimeFromUtc(Ephemeris.FromJulianDay(nextJd), tz));
    }

    /// <summary>Julian day near <paramref name="guessJd"/> when the sidereal Sun is at <paramref name="targetLon"/> (to well under a second).</summary>
    public static double SolarReturn(double targetLon, double guessJd)
    {
        var jd = guessJd;
        for (var i = 0; i < 20; i++)
        {
            var diff = Ephemeris.Norm(targetLon - Ephemeris.SiderealLongitude(Graha.Sun, jd) + 180) - 180;
            jd += diff / 0.9856;
            if (Math.Abs(diff) < 1e-6) break;
        }

        return jd;
    }

    /// <summary>Tajik aspects work by sign: 3/11 and 5/9 friendly, 1/4/7/10 strong; 2/6/8/12 have no aspect.</summary>
    private static bool Aspects(int fromRashi, int toRashi) => (((toRashi - fromRashi + 12) % 12) + 1) is not (2 or 6 or 8 or 12);

    private static int Strength(string dignity) => dignity switch
    {
        "Exalted" => 5,
        "Own sign" => 4,
        "Friendly" => 3,
        "Neutral" => 2,
        "Enemy sign" => 1,
        _ => 0
    };

    /// <summary>
    /// Mudda (Vimshottari compressed into one year). The first lord: (birth nakshatra number + completed years − 2) mod 9,
    /// counting 1 Sun, 2 Moon, 3 Mars, 4 Rahu, 5 Jupiter, 6 Saturn, 7 Mercury, 8 Ketu, 0 Venus. Periods split the year by Vimshottari years.
    /// </summary>
    public static List<MuddaDasha> MuddaDashas(double natalMoonLon, int age, DateTime startUtc, DateTime endUtc, TimeZoneInfo tz)
    {
        var nakshatraNumber = Vedic.NakshatraOf(natalMoonLon) + 1;
        var remainder = (((nakshatraNumber + age - 2) % 9) + 9) % 9;
        var first = (remainder + 10) % 9; // remainder 1 (Sun) → index 2 in the Vimshottari table; 0 (Venus) → 1.
        var yearDays = (endUtc - startUtc).TotalDays;

        var result = new List<MuddaDasha>();
        var cursor = startUtc;
        for (var i = 0; i < 9; i++)
        {
            var (lord, years) = Vedic.Vimshottari[(first + i) % 9];
            var end = i == 8 ? endUtc : cursor.AddDays(yearDays * years / 120.0);
            result.Add(new MuddaDasha(lord.ToString(), TimeZoneInfo.ConvertTimeFromUtc(cursor, tz), TimeZoneInfo.ConvertTimeFromUtc(end, tz), (int)Math.Round((end - cursor).TotalDays)));
            cursor = end;
        }

        return result;
    }
}
