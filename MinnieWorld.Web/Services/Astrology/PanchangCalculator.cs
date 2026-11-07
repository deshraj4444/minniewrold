namespace MinnieWorld.Web.Services.Astrology;

public record PanchangElements(string Tithi, int TithiNumber, string Paksha, string Nakshatra, string Yoga, string Karana);

public record TimedElement(string Name, DateTime? EndsAt);

public record TimeRange(DateTime Start, DateTime End);

public record Panchang(
    DateOnly Date, string Place, string TimeZone, string Vara,
    DateTime Sunrise, DateTime Sunset,
    TimedElement Tithi, string Paksha, TimedElement Nakshatra, TimedElement Yoga, TimedElement Karana,
    string SunSign, string MoonSign,
    string AmantaMonth, string PurnimantaMonth, int VikramSamvat, int ShakaSamvat, string Ritu,
    TimeRange RahuKaal, TimeRange Gulika, TimeRange Yamaganda, TimeRange AbhijitMuhurta, TimeRange BrahmaMuhurta);

/// <summary>Daily Panchang computed at local sunrise. All times returned are local to the requested time zone.</summary>
public static class PanchangCalculator
{
    private const double Deg = Math.PI / 180;

    public static readonly string[] Vara = ["Ravivara (Sunday)", "Somavara (Monday)", "Mangalavara (Tuesday)", "Budhavara (Wednesday)", "Guruvara (Thursday)", "Shukravara (Friday)", "Shanivara (Saturday)"];

    private static readonly string[] TithiNames =
    [
        "Pratipada", "Dwitiya", "Tritiya", "Chaturthi", "Panchami", "Shashthi", "Saptami", "Ashtami",
        "Navami", "Dashami", "Ekadashi", "Dwadashi", "Trayodashi", "Chaturdashi"
    ];

    private static readonly string[] YogaNames =
    [
        "Vishkambha", "Priti", "Ayushman", "Saubhagya", "Shobhana", "Atiganda", "Sukarma", "Dhriti", "Shula",
        "Ganda", "Vriddhi", "Dhruva", "Vyaghata", "Harshana", "Vajra", "Siddhi", "Vyatipata", "Variyana",
        "Parigha", "Shiva", "Siddha", "Sadhya", "Shubha", "Shukla", "Brahma", "Indra", "Vaidhriti"
    ];

    private static readonly string[] MovableKaranas = ["Bava", "Balava", "Kaulava", "Taitila", "Gara", "Vanija", "Vishti (Bhadra)"];

    public static readonly string[] Months = ["Chaitra", "Vaishakha", "Jyeshtha", "Ashadha", "Shravana", "Bhadrapada", "Ashwin", "Kartik", "Margashirsha", "Pausha", "Magha", "Phalguna"];

    private static readonly string[] Ritus = ["Vasanta (Spring)", "Grishma (Summer)", "Varsha (Monsoon)", "Sharad (Autumn)", "Hemanta (Pre-winter)", "Shishira (Winter)"];

    // 1-based eighth of the daytime for each weekday (Sunday first).
    private static readonly int[] RahuSegment = [8, 2, 7, 5, 6, 4, 3];
    private static readonly int[] GulikaSegment = [7, 6, 5, 4, 3, 2, 1];
    private static readonly int[] YamagandaSegment = [5, 4, 3, 2, 1, 7, 6];

    private static double Elongation(double jd) =>
        Ephemeris.Norm(Ephemeris.TropicalLongitude(Graha.Moon, jd) - Ephemeris.TropicalLongitude(Graha.Sun, jd));

    private static int TithiIndex(double jd) => (int)(Elongation(jd) / 12);
    private static int KaranaIndex(double jd) => (int)(Elongation(jd) / 6);
    private static int NakshatraIndex(double jd) => Vedic.NakshatraOf(Ephemeris.SiderealLongitude(Graha.Moon, jd));
    private static int YogaIndex(double jd) =>
        (int)(Ephemeris.Norm(Ephemeris.SiderealLongitude(Graha.Sun, jd) + Ephemeris.SiderealLongitude(Graha.Moon, jd)) / Vedic.NakshatraSpan);

    private static string TithiName(int index) =>
        index == 14 ? "Purnima" : index == 29 ? "Amavasya" : TithiNames[index % 15];

    private static string KaranaName(int index) => index switch
    {
        0 => "Kimstughna",
        57 => "Shakuni",
        58 => "Chatushpada",
        59 => "Naga",
        _ => MovableKaranas[(index - 1) % 7]
    };

    public static PanchangElements ElementsAt(double jd)
    {
        var t = TithiIndex(jd);
        return new PanchangElements(TithiName(t), (t % 15) + 1, t < 15 ? "Shukla" : "Krishna",
            Vedic.Nakshatras[NakshatraIndex(jd)].Name, YogaNames[YogaIndex(jd)], KaranaName(KaranaIndex(jd)));
    }

    public static Panchang Calculate(DateOnly date, double latitude, double longitude, string timeZoneId, string place)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var localMidnightUtc = TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), tz);
        var jd0 = Ephemeris.JulianDay(localMidnightUtc);

        var sunrise = SunEvent(jd0 + 0.25, latitude, longitude, rising: true);
        var sunset = SunEvent(jd0 + 0.75, latitude, longitude, rising: false);
        var nextSunrise = SunEvent(jd0 + 1.25, latitude, longitude, rising: true);

        DateTime Local(double jd) => TimeZoneInfo.ConvertTimeFromUtc(Ephemeris.FromJulianDay(jd), tz);
        DateTime? EndOf(Func<double, int> index) => FindChange(index, sunrise) is { } jd ? Local(jd) : null;

        var tithi = TithiIndex(sunrise);
        var weekday = (int)date.DayOfWeek;
        var dayLength = sunset - sunrise;
        TimeRange Segment(int n) => new(Local(sunrise + (dayLength * (n - 1) / 8)), Local(sunrise + (dayLength * n / 8)));

        var muhurta = dayLength / 15;
        var nightMuhurta = (nextSunrise - sunset) / 15;

        var (amanta, purnimanta) = LunarMonths(sunrise, tithi);
        var monthIdx = Array.IndexOf(Months, amanta);
        var vikram = date.Year + (monthIdx >= 10 || (monthIdx == 9 && date.Month <= 2) ? 56 : 57);

        return new Panchang(
            date, place, timeZoneId, Vara[weekday],
            Local(sunrise), Local(sunset),
            new TimedElement(TithiName(tithi), EndOf(TithiIndex)), tithi < 15 ? "Shukla Paksha" : "Krishna Paksha",
            new TimedElement(Vedic.Nakshatras[NakshatraIndex(sunrise)].Name, EndOf(NakshatraIndex)),
            new TimedElement(YogaNames[YogaIndex(sunrise)], EndOf(YogaIndex)),
            new TimedElement(KaranaName(KaranaIndex(sunrise)), EndOf(KaranaIndex)),
            Vedic.Rashis[Vedic.RashiOf(Ephemeris.SiderealLongitude(Graha.Sun, sunrise))].Name,
            Vedic.Rashis[Vedic.RashiOf(Ephemeris.SiderealLongitude(Graha.Moon, sunrise))].Name,
            amanta, purnimanta, vikram, vikram - 135, Ritus[monthIdx / 2],
            Segment(RahuSegment[weekday]), Segment(GulikaSegment[weekday]), Segment(YamagandaSegment[weekday]),
            new TimeRange(Local(sunrise + (muhurta * 7)), Local(sunrise + (muhurta * 8))),
            // Brahma muhurta: the 14th of the night's 15 muhurtas, i.e. the two muhurtas (≈96 min) before sunrise, first of them.
            new TimeRange(Local(sunrise - (nightMuhurta * 2)), Local(sunrise - nightMuhurta)));
    }

    /// <summary>Finds the first moment after <paramref name="startJd"/> where <paramref name="index"/> changes (to ~1 minute).</summary>
    private static double? FindChange(Func<double, int> index, double startJd)
    {
        var current = index(startJd);
        const double step = 1.0 / 24;
        for (var jd = startJd + step; jd <= startJd + 1.6; jd += step)
        {
            if (index(jd) == current) continue;
            double lo = jd - step, hi = jd;
            while (hi - lo > 1.0 / 1440)
            {
                var mid = (lo + hi) / 2;
                if (index(mid) == current) lo = mid; else hi = mid;
            }

            return hi;
        }

        return null;
    }

    /// <summary>Amanta month = named from the Sun's sidereal sign at the preceding new moon; Purnimanta runs a fortnight ahead in Krishna paksha.</summary>
    private static (string Amanta, string Purnimanta) LunarMonths(double jd, int tithiIndex)
    {
        var newMoon = jd - (Elongation(jd) / 12.19);
        for (var i = 0; i < 5; i++)
        {
            var e = Elongation(newMoon);
            if (e > 180) e -= 360;
            newMoon -= e / 12.19;
        }

        var sunSign = Vedic.RashiOf(Ephemeris.SiderealLongitude(Graha.Sun, newMoon));
        var amanta = (sunSign + 1) % 12;
        var purnimanta = tithiIndex >= 15 ? (amanta + 1) % 12 : amanta;
        return (Months[amanta], Months[purnimanta]);
    }

    /// <summary>Sunrise/sunset (upper limb with refraction, −0.833°) by iterating on the hour angle from a local-time guess.</summary>
    private static double SunEvent(double guessJd, double lat, double lon, bool rising)
    {
        var jd = guessJd;
        for (var i = 0; i < 6; i++)
        {
            var t = (jd - 2451545.0) / 36525.0;
            var lambda = Ephemeris.TropicalLongitude(Graha.Sun, jd) * Deg;
            var eps = Ephemeris.Obliquity(t) * Deg;
            var ra = Math.Atan2(Math.Cos(eps) * Math.Sin(lambda), Math.Cos(lambda)) / Deg;
            var dec = Math.Asin(Math.Sin(eps) * Math.Sin(lambda));
            var cosH0 = (Math.Sin(-0.833 * Deg) - (Math.Sin(lat * Deg) * Math.Sin(dec))) / (Math.Cos(lat * Deg) * Math.Cos(dec));
            cosH0 = Math.Clamp(cosH0, -1, 1); // polar day/night: clamp to noon/midnight
            var h0 = Math.Acos(cosH0) / Deg;

            var gmst = 280.46061837 + (360.98564736629 * (jd - 2451545.0));
            var h = Ephemeris.Norm(gmst + lon - ra);
            if (h > 180) h -= 360;
            var target = rising ? -h0 : h0;
            var diff = target - h;
            if (diff > 180) diff -= 360;
            if (diff < -180) diff += 360;
            jd += diff / 360.9856;
            if (Math.Abs(diff) < 0.001) break;
        }

        return jd;
    }
}
