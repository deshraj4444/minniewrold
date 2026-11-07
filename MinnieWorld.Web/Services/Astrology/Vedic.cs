namespace MinnieWorld.Web.Services.Astrology;

/// <summary>Static Jyotish reference data: rashis, nakshatras, lordships and dignities.</summary>
public static class Vedic
{
    public const double NakshatraSpan = 360.0 / 27;
    public const double PadaSpan = NakshatraSpan / 4;

    public record RashiInfo(int Index, string Name, string English, string Symbol, Graha Lord, string Element);

    public static readonly RashiInfo[] Rashis =
    [
        new(0, "Mesha", "Aries", "♈", Graha.Mars, "Fire"),
        new(1, "Vrishabha", "Taurus", "♉", Graha.Venus, "Earth"),
        new(2, "Mithuna", "Gemini", "♊", Graha.Mercury, "Air"),
        new(3, "Karka", "Cancer", "♋", Graha.Moon, "Water"),
        new(4, "Simha", "Leo", "♌", Graha.Sun, "Fire"),
        new(5, "Kanya", "Virgo", "♍", Graha.Mercury, "Earth"),
        new(6, "Tula", "Libra", "♎", Graha.Venus, "Air"),
        new(7, "Vrishchika", "Scorpio", "♏", Graha.Mars, "Water"),
        new(8, "Dhanu", "Sagittarius", "♐", Graha.Jupiter, "Fire"),
        new(9, "Makara", "Capricorn", "♑", Graha.Saturn, "Earth"),
        new(10, "Kumbha", "Aquarius", "♒", Graha.Saturn, "Air"),
        new(11, "Meena", "Pisces", "♓", Graha.Jupiter, "Water")
    ];

    public record NakshatraInfo(int Index, string Name, Graha Lord, string Deity, string Gana, string Yoni, string Nadi);

    // Lords follow the Vimshottari cycle Ketu → Venus → Sun → Moon → Mars → Rahu → Jupiter → Saturn → Mercury.
    public static readonly NakshatraInfo[] Nakshatras =
    [
        new(0, "Ashwini", Graha.Ketu, "Ashwini Kumaras", "Deva", "Horse", "Adi"),
        new(1, "Bharani", Graha.Venus, "Yama", "Manushya", "Elephant", "Madhya"),
        new(2, "Krittika", Graha.Sun, "Agni", "Rakshasa", "Sheep", "Antya"),
        new(3, "Rohini", Graha.Moon, "Brahma", "Manushya", "Serpent", "Antya"),
        new(4, "Mrigashira", Graha.Mars, "Soma", "Deva", "Serpent", "Madhya"),
        new(5, "Ardra", Graha.Rahu, "Rudra", "Manushya", "Dog", "Adi"),
        new(6, "Punarvasu", Graha.Jupiter, "Aditi", "Deva", "Cat", "Adi"),
        new(7, "Pushya", Graha.Saturn, "Brihaspati", "Deva", "Sheep", "Madhya"),
        new(8, "Ashlesha", Graha.Mercury, "Nagas", "Rakshasa", "Cat", "Antya"),
        new(9, "Magha", Graha.Ketu, "Pitris", "Rakshasa", "Rat", "Antya"),
        new(10, "Purva Phalguni", Graha.Venus, "Bhaga", "Manushya", "Rat", "Madhya"),
        new(11, "Uttara Phalguni", Graha.Sun, "Aryaman", "Manushya", "Cow", "Adi"),
        new(12, "Hasta", Graha.Moon, "Savitar", "Deva", "Buffalo", "Adi"),
        new(13, "Chitra", Graha.Mars, "Vishvakarma", "Rakshasa", "Tiger", "Madhya"),
        new(14, "Swati", Graha.Rahu, "Vayu", "Deva", "Buffalo", "Antya"),
        new(15, "Vishakha", Graha.Jupiter, "Indra-Agni", "Rakshasa", "Tiger", "Antya"),
        new(16, "Anuradha", Graha.Saturn, "Mitra", "Deva", "Deer", "Madhya"),
        new(17, "Jyeshtha", Graha.Mercury, "Indra", "Rakshasa", "Deer", "Adi"),
        new(18, "Mula", Graha.Ketu, "Nirriti", "Rakshasa", "Dog", "Adi"),
        new(19, "Purva Ashadha", Graha.Venus, "Apas", "Manushya", "Monkey", "Madhya"),
        new(20, "Uttara Ashadha", Graha.Sun, "Vishvedevas", "Manushya", "Mongoose", "Antya"),
        new(21, "Shravana", Graha.Moon, "Vishnu", "Deva", "Monkey", "Antya"),
        new(22, "Dhanishta", Graha.Mars, "Vasus", "Rakshasa", "Lion", "Madhya"),
        new(23, "Shatabhisha", Graha.Rahu, "Varuna", "Rakshasa", "Horse", "Adi"),
        new(24, "Purva Bhadrapada", Graha.Jupiter, "Aja Ekapada", "Manushya", "Lion", "Adi"),
        new(25, "Uttara Bhadrapada", Graha.Saturn, "Ahir Budhnya", "Manushya", "Cow", "Madhya"),
        new(26, "Revati", Graha.Mercury, "Pushan", "Deva", "Elephant", "Antya")
    ];

    public static readonly Dictionary<Graha, string> GrahaNames = new()
    {
        [Graha.Sun] = "Surya", [Graha.Moon] = "Chandra", [Graha.Mars] = "Mangal", [Graha.Mercury] = "Budh",
        [Graha.Jupiter] = "Guru", [Graha.Venus] = "Shukra", [Graha.Saturn] = "Shani", [Graha.Rahu] = "Rahu", [Graha.Ketu] = "Ketu"
    };

    public static readonly Dictionary<Graha, string> GrahaAbbr = new()
    {
        [Graha.Sun] = "Su", [Graha.Moon] = "Mo", [Graha.Mars] = "Ma", [Graha.Mercury] = "Me",
        [Graha.Jupiter] = "Ju", [Graha.Venus] = "Ve", [Graha.Saturn] = "Sa", [Graha.Rahu] = "Ra", [Graha.Ketu] = "Ke"
    };

    // Exaltation sign index for each graha; debilitation is the opposite sign.
    private static readonly Dictionary<Graha, int> Exaltation = new()
    {
        [Graha.Sun] = 0, [Graha.Moon] = 1, [Graha.Mars] = 9, [Graha.Mercury] = 5, [Graha.Jupiter] = 3,
        [Graha.Venus] = 11, [Graha.Saturn] = 6, [Graha.Rahu] = 1, [Graha.Ketu] = 7
    };

    public static string Dignity(Graha graha, int rashi)
    {
        var ex = Exaltation[graha];
        if (rashi == ex) return "Exalted";
        if (rashi == (ex + 6) % 12) return "Debilitated";
        if (graha is not (Graha.Rahu or Graha.Ketu) && Rashis[rashi].Lord == graha) return "Own sign";
        return Relationship(graha, Rashis[rashi].Lord) switch
        {
            Friendship.Friend => "Friendly",
            Friendship.Enemy => "Enemy sign",
            _ => "Neutral"
        };
    }

    public enum Friendship { Friend, Neutral, Enemy }

    private static readonly Dictionary<Graha, (Graha[] Friends, Graha[] Enemies)> Natural = new()
    {
        [Graha.Sun] = ([Graha.Moon, Graha.Mars, Graha.Jupiter], [Graha.Venus, Graha.Saturn]),
        [Graha.Moon] = ([Graha.Sun, Graha.Mercury], []),
        [Graha.Mars] = ([Graha.Sun, Graha.Moon, Graha.Jupiter], [Graha.Mercury]),
        [Graha.Mercury] = ([Graha.Sun, Graha.Venus], [Graha.Moon]),
        [Graha.Jupiter] = ([Graha.Sun, Graha.Moon, Graha.Mars], [Graha.Mercury, Graha.Venus]),
        [Graha.Venus] = ([Graha.Mercury, Graha.Saturn], [Graha.Sun, Graha.Moon]),
        [Graha.Saturn] = ([Graha.Mercury, Graha.Venus], [Graha.Sun, Graha.Moon, Graha.Mars]),
        [Graha.Rahu] = ([Graha.Mercury, Graha.Venus, Graha.Saturn], [Graha.Sun, Graha.Moon, Graha.Mars]),
        [Graha.Ketu] = ([Graha.Mars, Graha.Venus, Graha.Saturn], [Graha.Sun, Graha.Moon])
    };

    /// <summary>Naisargika (natural) relationship: how <paramref name="of"/> regards <paramref name="towards"/>.</summary>
    public static Friendship Relationship(Graha of, Graha towards)
    {
        if (of == towards) return Friendship.Friend;
        var (friends, enemies) = Natural[of];
        return friends.Contains(towards) ? Friendship.Friend : enemies.Contains(towards) ? Friendship.Enemy : Friendship.Neutral;
    }

    // Vimshottari: dasha lord order and years (total 120).
    public static readonly (Graha Lord, int Years)[] Vimshottari =
    [
        (Graha.Ketu, 7), (Graha.Venus, 20), (Graha.Sun, 6), (Graha.Moon, 10), (Graha.Mars, 7),
        (Graha.Rahu, 18), (Graha.Jupiter, 16), (Graha.Saturn, 19), (Graha.Mercury, 17)
    ];

    public static int RashiOf(double siderealLongitude) => (int)(Ephemeris.Norm(siderealLongitude) / 30) % 12;

    public static int NakshatraOf(double siderealLongitude) => (int)(Ephemeris.Norm(siderealLongitude) / NakshatraSpan) % 27;

    public static int PadaOf(double siderealLongitude) => (int)(Ephemeris.Norm(siderealLongitude) % NakshatraSpan / PadaSpan) + 1;

    /// <summary>24.5° → "24°30'".</summary>
    public static string Dms(double degrees)
    {
        var d = (int)degrees;
        var m = (int)Math.Round((degrees - d) * 60);
        if (m == 60) { d += 1; m = 0; }
        return $"{d}°{m:00}'";
    }
}
