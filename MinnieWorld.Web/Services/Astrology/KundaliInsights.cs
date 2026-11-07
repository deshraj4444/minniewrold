namespace MinnieWorld.Web.Services.Astrology;

public record YogaResult(string Name, string Description);

public record KalsarpResult(bool Present, string Name, string Summary);

public record SadeSatiPeriod(DateOnly Start, DateOnly End, bool IsCurrent);

public record SadeSatiResult(
    string MoonSign, string SaturnSignNow, bool InSadeSati, string Phase, bool InDhaiya, string Summary,
    List<SadeSatiPeriod> Periods);

public record TransitPosition(string Graha, string RashiName, string Degree, int HouseFromMoon, bool Favourable, bool Retrograde);

public record KundaliInsights(
    List<YogaResult> Yogas, KalsarpResult Kalsarp, SadeSatiResult SadeSati,
    List<ChartHouse> DashamsaChart, DateTime TransitDate, List<TransitPosition> Transits);

/// <summary>Yogas, doshas, Sade Sati, the D10 chart and current transits (gochar) for a birth chart.</summary>
public static class KundaliInsightsCalculator
{
    private static readonly string[] KalsarpNames =
        ["Anant", "Kulik", "Vasuki", "Shankhpal", "Padma", "Mahapadma", "Takshak", "Karkotak", "Shankhachud", "Ghatak", "Vishdhar", "Sheshnag"];

    // Houses from the natal Moon where a transiting graha gives good results (classical gochara).
    private static readonly Dictionary<string, int[]> GoodTransitHouses = new()
    {
        ["Sun"] = [3, 6, 10, 11],
        ["Moon"] = [1, 3, 6, 7, 10, 11],
        ["Mars"] = [3, 6, 11],
        ["Mercury"] = [2, 4, 6, 8, 10, 11],
        ["Jupiter"] = [2, 5, 7, 9, 11],
        ["Venus"] = [1, 2, 3, 4, 5, 8, 9, 11, 12],
        ["Saturn"] = [3, 6, 11],
        ["Rahu"] = [3, 6, 11],
        ["Ketu"] = [3, 6, 11]
    };

    public static KundaliInsights Calculate(List<PlanetPosition> planets, PlanetPosition lagna, DateTime birthUtc, DateTime nowUtc)
    {
        var moon = planets.First(p => p.Graha == "Moon");
        return new KundaliInsights(
            Yogas(planets, lagna),
            Kalsarp(planets, lagna),
            SadeSati(moon.Rashi, birthUtc, nowUtc),
            KundaliCalculator.BuildChart(Dashamsa(lagna.Longitude), planets, p => Dashamsa(p.Longitude)),
            nowUtc,
            Transits(moon.Rashi, nowUtc));
    }

    /// <summary>D10: 3° parts, counted from the sign itself for odd signs and from the 9th sign for even signs.</summary>
    public static int Dashamsa(double lon)
    {
        var rashi = Vedic.RashiOf(lon);
        var part = (int)(lon % 30 / 3);
        var start = rashi % 2 == 0 ? rashi : (rashi + 8) % 12;
        return (start + part) % 12;
    }

    private static int HouseFrom(int fromRashi, int rashi) => ((rashi - fromRashi + 12) % 12) + 1;

    private static List<YogaResult> Yogas(List<PlanetPosition> planets, PlanetPosition lagna)
    {
        PlanetPosition P(string g) => planets.First(p => p.Graha == g);
        var moon = P("Moon");
        var yogas = new List<YogaResult>();
        int[] kendras = [1, 4, 7, 10];

        if (kendras.Contains(HouseFrom(moon.Rashi, P("Jupiter").Rashi)))
            yogas.Add(new("Gajakesari Yoga", "Jupiter is in a kendra (1st, 4th, 7th or 10th) from the Moon. Traditionally gives wisdom, reputation and lasting prosperity."));

        if (P("Sun").Rashi == P("Mercury").Rashi)
            yogas.Add(new("Budhaditya Yoga", "The Sun and Mercury are in the same sign. Associated with intelligence, good communication and learning."));

        if (moon.Rashi == P("Mars").Rashi)
            yogas.Add(new("Chandra-Mangal Yoga", "The Moon and Mars are together. Associated with drive and the ability to earn through one's own effort."));

        var mahapurusha = new (string Graha, string Name, string Gives)[]
        {
            ("Mars", "Ruchaka", "courage, leadership and physical strength"),
            ("Mercury", "Bhadra", "intellect, eloquence and business skill"),
            ("Jupiter", "Hamsa", "wisdom, righteousness and respect"),
            ("Venus", "Malavya", "comfort, art, beauty and a happy family life"),
            ("Saturn", "Shasha", "authority, discipline and influence over many people")
        };
        foreach (var (graha, name, gives) in mahapurusha)
        {
            var p = P(graha);
            if (p.Dignity is "Exalted" or "Own sign" && kendras.Contains(p.House))
                yogas.Add(new($"{name} Yoga (Pancha Mahapurusha)", $"{graha} is {p.Dignity.ToLowerInvariant()} in a kendra ({Ordinal(p.House)} house). Gives {gives}."));
        }

        // Sunapha / Anapha / Durudhara / Kemadruma: planets other than the Sun and the nodes in the 2nd and 12th from the Moon.
        var others = planets.Where(p => p.Graha is not ("Sun" or "Moon" or "Rahu" or "Ketu")).ToList();
        var second = others.Any(p => HouseFrom(moon.Rashi, p.Rashi) == 2);
        var twelfth = others.Any(p => HouseFrom(moon.Rashi, p.Rashi) == 12);
        if (second && twelfth)
            yogas.Add(new("Durudhara Yoga", "Planets on both sides of the Moon (2nd and 12th from it). Gives wealth, comforts and a generous nature."));
        else if (second)
            yogas.Add(new("Sunapha Yoga", "A planet in the 2nd from the Moon. Gives self-earned wealth and a good reputation."));
        else if (twelfth)
            yogas.Add(new("Anapha Yoga", "A planet in the 12th from the Moon. Gives good health, a pleasant personality and comforts."));
        else if (!others.Any(p => p.Rashi == moon.Rashi) && !others.Any(p => kendras.Contains(HouseFrom(lagna.Rashi, p.Rashi)) && p.Graha != "Moon"))
            yogas.Add(new("Kemadruma Yoga", "No planets on either side of the Moon and none in kendras from the Lagna. Traditionally indicates struggle; remedies for the Moon are often advised."));

        var lagnaLord = Vedic.Rashis[lagna.Rashi].Lord.ToString();
        var lord = P(lagnaLord);
        if (kendras.Contains(lord.House) || lord.House is 5 or 9)
            yogas.Add(new("Strong Lagna lord", $"The Lagna lord {lagnaLord} is in the {Ordinal(lord.House)} house (a kendra or trikona), which strengthens health and personality."));

        return yogas;
    }

    private static KalsarpResult Kalsarp(List<PlanetPosition> planets, PlanetPosition lagna)
    {
        var rahu = planets.First(p => p.Graha == "Rahu");
        var seven = planets.Where(p => p.Graha is not ("Rahu" or "Ketu")).ToList();
        var arcs = seven.Select(p => Ephemeris.Norm(p.Longitude - rahu.Longitude)).ToList();
        var oneSide = arcs.All(a => a < 180) || arcs.All(a => a > 180);
        if (!oneSide)
        {
            return new KalsarpResult(false, "", "No Kalsarp dosha: the planets are not all hemmed between Rahu and Ketu.");
        }

        var name = KalsarpNames[rahu.House - 1] + " Kalsarp";
        return new KalsarpResult(true, name,
            $"{name} dosha: all seven planets lie on one side of the Rahu–Ketu axis, with Rahu in the {Ordinal(rahu.House)} house. Its effects vary a lot with the rest of the chart; consult an astrologer before doing remedies.");
    }

    private static SadeSatiResult SadeSati(int moonRashi, DateTime birthUtc, DateTime nowUtc)
    {
        int[] sadeSati = [(moonRashi + 11) % 12, moonRashi, (moonRashi + 1) % 12];
        var nowJd = Ephemeris.JulianDay(nowUtc);
        var saturnNow = Vedic.RashiOf(Ephemeris.SiderealLongitude(Graha.Saturn, nowJd));
        var inSadeSati = sadeSati.Contains(saturnNow);
        var inDhaiya = saturnNow == (moonRashi + 3) % 12 || saturnNow == (moonRashi + 7) % 12;
        var phase = !inSadeSati ? "" : saturnNow == sadeSati[0] ? "Rising (first 2½ years)" : saturnNow == moonRashi ? "Peak (middle 2½ years)" : "Setting (last 2½ years)";

        // Find every stretch where Saturn is in the 12th, 1st or 2nd from the Moon, from birth for 100 years.
        var periods = new List<SadeSatiPeriod>();
        var start = Ephemeris.JulianDay(birthUtc);
        var end = start + (100 * 365.25);
        bool In(double jd) => sadeSati.Contains(Vedic.RashiOf(Ephemeris.SiderealLongitude(Graha.Saturn, jd)));

        var step = 10.0;
        var inside = In(start);
        var periodStart = inside ? start : (double?)null;
        for (var jd = start + step; jd <= end; jd += step)
        {
            var now = In(jd);
            if (now == inside) continue;
            var edge = Bisect(jd - step, jd, In, inside);
            if (now) periodStart = edge;
            else if (periodStart is { } s) { periods.Add(Period(s, edge, nowJd)); periodStart = null; }
            inside = now;
        }

        if (periodStart is { } open) periods.Add(Period(open, end, nowJd));

        // Retrograde Saturn can briefly leave and re-enter; join stretches separated by less than a year.
        var merged = new List<SadeSatiPeriod>();
        foreach (var p in periods)
        {
            if (merged.Count > 0 && p.Start.DayNumber - merged[^1].End.DayNumber < 365)
            {
                var last = merged[^1];
                merged[^1] = new SadeSatiPeriod(last.Start, p.End, last.IsCurrent || p.IsCurrent);
            }
            else
            {
                merged.Add(p);
            }
        }

        var moonName = Vedic.Rashis[moonRashi].Name;
        var saturnName = Vedic.Rashis[saturnNow].Name;
        var summary = inSadeSati
            ? $"Sade Sati is running: Saturn is transiting {saturnName}, the {Ordinal(HouseFrom(moonRashi, saturnNow))} from your Moon sign {moonName}. Phase: {phase.ToLowerInvariant()}."
            : inDhaiya
                ? $"No Sade Sati now, but Shani Dhaiya (Small Panoti) is running: Saturn is in {saturnName}, the {Ordinal(HouseFrom(moonRashi, saturnNow))} from your Moon sign."
                : $"No Sade Sati now. Saturn is transiting {saturnName}, the {Ordinal(HouseFrom(moonRashi, saturnNow))} from your Moon sign {moonName}.";

        return new SadeSatiResult(moonName, saturnName, inSadeSati, phase, inDhaiya, summary, merged);
    }

    private static SadeSatiPeriod Period(double startJd, double endJd, double nowJd) =>
        new(DateOnly.FromDateTime(Ephemeris.FromJulianDay(startJd)), DateOnly.FromDateTime(Ephemeris.FromJulianDay(endJd)), startJd <= nowJd && nowJd < endJd);

    /// <summary>Finds to within about an hour where <paramref name="test"/> stops returning <paramref name="before"/>.</summary>
    private static double Bisect(double lo, double hi, Func<double, bool> test, bool before)
    {
        while (hi - lo > 0.04)
        {
            var mid = (lo + hi) / 2;
            if (test(mid) == before) lo = mid;
            else hi = mid;
        }

        return hi;
    }

    private static List<TransitPosition> Transits(int moonRashi, DateTime nowUtc)
    {
        var jd = Ephemeris.JulianDay(nowUtc);
        return Enum.GetValues<Graha>().Select(g =>
        {
            var lon = Ephemeris.SiderealLongitude(g, jd);
            var rashi = Vedic.RashiOf(lon);
            var house = HouseFrom(moonRashi, rashi);
            var retro = g is Graha.Rahu or Graha.Ketu || (g is not (Graha.Sun or Graha.Moon) && Ephemeris.Speed(g, jd) < 0);
            return new TransitPosition(g.ToString(), Vedic.Rashis[rashi].Name, Vedic.Dms(lon % 30), house, GoodTransitHouses[g.ToString()].Contains(house), retro);
        }).ToList();
    }

    public static string Ordinal(int n) => n + (n % 100 is 11 or 12 or 13 ? "th" : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" });
}
