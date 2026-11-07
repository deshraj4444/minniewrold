namespace MinnieWorld.Web.Services.Astrology;

public enum Graha { Sun, Moon, Mars, Mercury, Jupiter, Venus, Saturn, Rahu, Ketu }

/// <summary>
/// Geocentric planetary longitudes from published low-precision algorithms:
/// Sun and Moon from Meeus, "Astronomical Algorithms" (ch. 25 and 47, ~0.01°);
/// planets from the JPL Keplerian elements valid 1800–2050 (typically within a few arc-minutes,
/// up to ~10' for Saturn). That is well inside a nakshatra pada (3°20'), which is the finest
/// division a kundali uses, but don't use it for astronomical work.
/// </summary>
public static class Ephemeris
{
    private const double Deg = Math.PI / 180;

    public static double JulianDay(DateTime utc)
    {
        var y = utc.Year;
        var m = utc.Month;
        var d = utc.Day + (utc.TimeOfDay.TotalHours / 24.0);
        if (m <= 2) { y -= 1; m += 12; }
        var a = Math.Floor(y / 100.0);
        var b = 2 - a + Math.Floor(a / 4);
        return Math.Floor(365.25 * (y + 4716)) + Math.Floor(30.6001 * (m + 1)) + d + b - 1524.5;
    }

    public static DateTime FromJulianDay(double jd) =>
        new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc).AddDays(jd - 2451545.0);

    /// <summary>TT − UT in days (Espenak–Meeus polynomials, 2005–2050 and 1986–2005; good enough elsewhere).</summary>
    public static double DeltaTDays(double jdUt)
    {
        var year = 2000 + ((jdUt - 2451545.0) / 365.25);
        double seconds;
        if (year >= 2005)
        {
            var t = year - 2000;
            seconds = 62.92 + (0.32217 * t) + (0.005589 * t * t);
        }
        else if (year >= 1986)
        {
            var t = year - 2000;
            seconds = 63.86 + (0.3345 * t) - (0.060374 * t * t) + (0.0017275 * t * t * t);
        }
        else if (year >= 1961)
        {
            var t = year - 1975;
            seconds = 45.45 + (1.067 * t) - (t * t / 260) - (t * t * t / 718);
        }
        else
        {
            var t = year - 1950;
            seconds = 29.07 + (0.407 * t) - (t * t / 233) + (t * t * t / 2547);
        }

        return seconds / 86400.0;
    }

    /// <summary>Lahiri (Chitrapaksha) ayanamsa in degrees.</summary>
    public static double LahiriAyanamsa(double jdUt)
    {
        var t = (jdUt - 2451545.0) / 36525.0;
        return 23.85709 + (1.396971 * t) + (0.0003086 * t * t);
    }

    /// <summary>Tropical geocentric longitude of date, degrees [0, 360).</summary>
    public static double TropicalLongitude(Graha graha, double jdUt)
    {
        var jde = jdUt + DeltaTDays(jdUt);
        var t = (jde - 2451545.0) / 36525.0;
        return graha switch
        {
            Graha.Sun => Sun(t),
            Graha.Moon => Moon(t),
            Graha.Rahu => MeanNode(t),
            Graha.Ketu => Norm(MeanNode(t) + 180),
            _ => Planet(graha, t)
        };
    }

    public static double SiderealLongitude(Graha graha, double jdUt) =>
        Norm(TropicalLongitude(graha, jdUt) - LahiriAyanamsa(jdUt));

    /// <summary>Daily motion in degrees (negative = retrograde).</summary>
    public static double Speed(Graha graha, double jdUt)
    {
        var diff = TropicalLongitude(graha, jdUt + 0.5) - TropicalLongitude(graha, jdUt - 0.5);
        if (diff > 180) diff -= 360;
        if (diff < -180) diff += 360;
        return diff;
    }

    /// <summary>Sidereal ascendant (Lagna) for a place; longitude east-positive.</summary>
    public static double SiderealAscendant(double jdUt, double latitude, double longitude)
    {
        var t = (jdUt - 2451545.0) / 36525.0;
        var gmst = 280.46061837 + (360.98564736629 * (jdUt - 2451545.0)) + (0.000387933 * t * t) - (t * t * t / 38710000.0);
        var ramc = Norm(gmst + longitude) * Deg;
        var eps = Obliquity(t) * Deg;
        var asc = Math.Atan2(Math.Cos(ramc), -((Math.Sin(ramc) * Math.Cos(eps)) + (Math.Tan(latitude * Deg) * Math.Sin(eps)))) / Deg;
        return Norm(asc - LahiriAyanamsa(jdUt));
    }

    public static double Obliquity(double t) => 23.4392911 - (0.0130042 * t) - (1.64e-7 * t * t) + (5.04e-7 * t * t * t);

    public static double Norm(double degrees)
    {
        var x = degrees % 360;
        return x < 0 ? x + 360 : x;
    }

    private static double Sun(double t)
    {
        var l0 = 280.46646 + (36000.76983 * t) + (0.0003032 * t * t);
        var m = (357.52911 + (35999.05029 * t) - (0.0001537 * t * t)) * Deg;
        var c = ((1.914602 - (0.004817 * t) - (0.000014 * t * t)) * Math.Sin(m))
              + ((0.019993 - (0.000101 * t)) * Math.Sin(2 * m))
              + (0.000289 * Math.Sin(3 * m));
        var omega = (125.04 - (1934.136 * t)) * Deg;
        return Norm(l0 + c - 0.00569 - (0.00478 * Math.Sin(omega)));
    }

    private static double MeanNode(double t) =>
        Norm(125.0445479 - (1934.1362891 * t) + (0.0020754 * t * t) + (t * t * t / 467441.0));

    // Meeus table 47.A: multiples of D, M, M', F and the sine coefficient (1e-6 degrees).
    private static readonly int[,] MoonTerms =
    {
        { 0, 0, 1, 0, 6288774 }, { 2, 0, -1, 0, 1274027 }, { 2, 0, 0, 0, 658314 }, { 0, 0, 2, 0, 213618 },
        { 0, 1, 0, 0, -185116 }, { 0, 0, 0, 2, -114332 }, { 2, 0, -2, 0, 58793 }, { 2, -1, -1, 0, 57066 },
        { 2, 0, 1, 0, 53322 }, { 2, -1, 0, 0, 45758 }, { 0, 1, -1, 0, -40923 }, { 1, 0, 0, 0, -34720 },
        { 0, 1, 1, 0, -30383 }, { 2, 0, 0, -2, 15327 }, { 0, 0, 1, 2, -12528 }, { 0, 0, 1, -2, 10980 },
        { 4, 0, -1, 0, 10675 }, { 0, 0, 3, 0, 10034 }, { 4, 0, -2, 0, 8548 }, { 2, 1, -1, 0, -7888 },
        { 2, 1, 0, 0, -6766 }, { 1, 0, -1, 0, -5163 }, { 1, 1, 0, 0, 4987 }, { 2, -1, 1, 0, 4036 },
        { 2, 0, 2, 0, 3994 }, { 4, 0, 0, 0, 3861 }, { 2, 0, -3, 0, 3665 }, { 0, 1, -2, 0, -2689 },
        { 2, 0, -1, 2, -2602 }, { 2, -1, -2, 0, 2390 }, { 1, 0, 1, 0, -2348 }, { 2, -2, 0, 0, 2236 },
        { 0, 1, 2, 0, -2120 }, { 0, 2, 0, 0, -2069 }, { 2, -2, -1, 0, 2048 }, { 2, 0, 1, -2, -1773 },
        { 2, 0, 0, 2, -1595 }, { 4, -1, -1, 0, 1215 }, { 0, 0, 2, 2, -1110 }, { 3, 0, -1, 0, -892 },
        { 2, 1, 1, 0, -810 }, { 4, -1, -2, 0, 759 }, { 0, 2, -1, 0, -713 }, { 2, 2, -1, 0, -700 },
        { 2, 1, -2, 0, 691 }, { 2, -1, 0, -2, 596 }, { 4, 0, 1, 0, 549 }, { 0, 0, 4, 0, 537 },
        { 4, -1, 0, 0, 520 }, { 1, 0, -2, 0, -487 }, { 2, 1, 0, -2, -399 }, { 0, 0, 2, -2, -381 },
        { 1, 1, 1, 0, 351 }, { 3, 0, -2, 0, -340 }, { 4, 0, -3, 0, 330 }, { 2, -1, 2, 0, 327 },
        { 0, 2, 1, 0, -323 }, { 1, 1, -1, 0, 299 }, { 2, 0, 3, 0, 294 }
    };

    private static double Moon(double t)
    {
        var t2 = t * t; var t3 = t2 * t; var t4 = t3 * t;
        var lp = 218.3164477 + (481267.88123421 * t) - (0.0015786 * t2) + (t3 / 538841) - (t4 / 65194000);
        var d = 297.8501921 + (445267.1114034 * t) - (0.0018819 * t2) + (t3 / 545868) - (t4 / 113065000);
        var m = 357.5291092 + (35999.0502909 * t) - (0.0001536 * t2) + (t3 / 24490000);
        var mp = 134.9633964 + (477198.8675055 * t) + (0.0087414 * t2) + (t3 / 69699) - (t4 / 14712000);
        var f = 93.2720950 + (483202.0175233 * t) - (0.0036539 * t2) - (t3 / 3526000) + (t4 / 863310000);
        var a1 = 119.75 + (131.849 * t);
        var a2 = 53.09 + (479264.290 * t);
        var e = 1 - (0.002516 * t) - (0.0000074 * t2);

        double sum = 0;
        for (var i = 0; i < MoonTerms.GetLength(0); i++)
        {
            var mMult = MoonTerms[i, 1];
            var arg = ((MoonTerms[i, 0] * d) + (mMult * m) + (MoonTerms[i, 2] * mp) + (MoonTerms[i, 3] * f)) * Deg;
            var coeff = MoonTerms[i, 4] * (Math.Abs(mMult) == 1 ? e : Math.Abs(mMult) == 2 ? e * e : 1);
            sum += coeff * Math.Sin(arg);
        }

        sum += (3958 * Math.Sin(a1 * Deg)) + (1962 * Math.Sin((lp - f) * Deg)) + (318 * Math.Sin(a2 * Deg));
        return Norm(lp + (sum / 1_000_000));
    }

    // JPL "Approximate Positions of the Planets", table 1 (1800–2050):
    // a (AU), e, I (°), L (°), long. perihelion (°), long. node (°), then the same per Julian century.
    private static readonly Dictionary<string, double[]> Elements = new()
    {
        ["Mercury"] = [0.38709927, 0.20563593, 7.00497902, 252.25032350, 77.45779628, 48.33076593, 0.00000037, 0.00001906, -0.00594749, 149472.67411175, 0.16047689, -0.12534081],
        ["Venus"] = [0.72333566, 0.00677672, 3.39467605, 181.97909950, 131.60246718, 76.67984255, 0.00000390, -0.00004107, -0.00078890, 58517.81538729, 0.00268329, -0.27769418],
        ["Earth"] = [1.00000261, 0.01671123, -0.00001531, 100.46457166, 102.93768193, 0.0, 0.00000562, -0.00004392, -0.01294668, 35999.37244981, 0.32327364, 0.0],
        ["Mars"] = [1.52371034, 0.09339410, 1.84969142, -4.55343205, -23.94362959, 49.55953891, 0.00001847, 0.00007882, -0.00813131, 19140.30268499, 0.44441088, -0.29257343],
        ["Jupiter"] = [5.20288700, 0.04838624, 1.30439695, 34.39644051, 14.72847983, 100.47390909, -0.00011607, -0.00013253, -0.00183714, 3034.74612775, 0.21252668, 0.20469106],
        ["Saturn"] = [9.53667594, 0.05386179, 2.48599187, 49.95424423, 92.59887831, 113.66242448, -0.00125060, -0.00050991, 0.00193609, 1222.49362201, -0.41897216, -0.28867794]
    };

    private static double Planet(Graha graha, double t)
    {
        var (px, py, pz) = Heliocentric(graha.ToString(), t);
        var (ex, ey, ez) = Heliocentric("Earth", t);

        // First pass gives the distance; redo at (t − light time) so fast inner planets land in the right place.
        var dist = Math.Sqrt(Math.Pow(px - ex, 2) + Math.Pow(py - ey, 2) + Math.Pow(pz - ez, 2));
        (px, py, _) = Heliocentric(graha.ToString(), t - (0.0057755183 * dist / 36525.0));

        var lonJ2000 = Math.Atan2(py - ey, px - ex) / Deg;
        // Precess from the J2000 ecliptic to the ecliptic of date (general precession in longitude).
        return Norm(lonJ2000 + (1.396971 * t) + (0.0003086 * t * t));
    }

    private static (double X, double Y, double Z) Heliocentric(string body, double t)
    {
        var el = Elements[body];
        var a = el[0] + (el[6] * t);
        var e = el[1] + (el[7] * t);
        var i = (el[2] + (el[8] * t)) * Deg;
        var l = el[3] + (el[9] * t);
        var peri = el[4] + (el[10] * t);
        var node = el[5] + (el[11] * t);

        var w = (peri - node) * Deg;
        var m = Norm(l - peri);
        if (m > 180) m -= 360;
        m *= Deg;

        // Kepler's equation by Newton iteration.
        var ecc = m + (e * Math.Sin(m));
        for (var k = 0; k < 10; k++)
        {
            var delta = (ecc - (e * Math.Sin(ecc)) - m) / (1 - (e * Math.Cos(ecc)));
            ecc -= delta;
            if (Math.Abs(delta) < 1e-10) break;
        }

        var xp = a * (Math.Cos(ecc) - e);
        var yp = a * Math.Sqrt(1 - (e * e)) * Math.Sin(ecc);
        var o = node * Deg;

        var x = (((Math.Cos(w) * Math.Cos(o)) - (Math.Sin(w) * Math.Sin(o) * Math.Cos(i))) * xp)
              + (((-Math.Sin(w) * Math.Cos(o)) - (Math.Cos(w) * Math.Sin(o) * Math.Cos(i))) * yp);
        var y = (((Math.Cos(w) * Math.Sin(o)) + (Math.Sin(w) * Math.Cos(o) * Math.Cos(i))) * xp)
              + (((-Math.Sin(w) * Math.Sin(o)) + (Math.Cos(w) * Math.Cos(o) * Math.Cos(i))) * yp);
        var z = (Math.Sin(w) * Math.Sin(i) * xp) + (Math.Cos(w) * Math.Sin(i) * yp);
        return (x, y, z);
    }
}
