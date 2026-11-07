namespace MinnieWorld.Web.Services.Astrology;

public record KootaScore(string Name, double Obtained, double Maximum, string Boy, string Girl, string Meaning);

public record MatchPerson(string Name, string Rashi, string Nakshatra, int Pada, ManglikResult Manglik);

public record MatchResult(MatchPerson Boy, MatchPerson Girl, List<KootaScore> Kootas, double Total, string Verdict, string ManglikNote);

/// <summary>Ashtakoota (36-guna) compatibility from the Moon's rashi and nakshatra of both people.</summary>
public static class Matching
{
    private static readonly string[] VarnaNames = ["Shudra", "Vaishya", "Kshatriya", "Brahmin"];
    // Rashi index → varna rank (0 = Shudra … 3 = Brahmin).
    private static readonly int[] VarnaRank = [2, 1, 0, 3, 2, 1, 0, 3, 2, 1, 0, 3];

    public static string Varna(int rashi) => VarnaNames[VarnaRank[rashi]];

    private static readonly string[] VashyaNames = ["Chatushpada", "Manava", "Jalachara", "Vanachara", "Keeta"];

    private static int VashyaIndex(double lon)
    {
        var rashi = Vedic.RashiOf(lon);
        var firstHalf = lon % 30 < 15;
        return rashi switch
        {
            0 or 1 => 0,
            2 or 5 or 6 or 10 => 1,
            3 or 11 => 2,
            4 => 3,
            7 => 4,
            8 => firstHalf ? 1 : 0,
            9 => firstHalf ? 0 : 2,
            _ => 1
        };
    }

    public static string Vashya(double lon) => VashyaNames[VashyaIndex(lon)];

    // Rows: boy's vashya, columns: girl's vashya.
    private static readonly double[,] VashyaTable =
    {
        { 2, 1, 1, 0.5, 1 },
        { 1, 2, 0.5, 0, 1 },
        { 1, 0.5, 2, 1, 1 },
        { 0.5, 0, 1, 2, 0 },
        { 1, 1, 1, 0, 2 }
    };

    private static readonly string[] Yonis = ["Horse", "Elephant", "Sheep", "Serpent", "Dog", "Cat", "Rat", "Cow", "Buffalo", "Tiger", "Deer", "Monkey", "Mongoose", "Lion"];

    private static readonly int[,] YoniTable =
    {
        { 4, 2, 2, 3, 2, 2, 2, 1, 0, 1, 3, 3, 2, 1 },
        { 2, 4, 3, 3, 2, 2, 2, 2, 3, 1, 2, 3, 2, 0 },
        { 2, 3, 4, 2, 1, 2, 1, 3, 3, 1, 2, 0, 3, 1 },
        { 3, 3, 2, 4, 2, 1, 1, 1, 1, 2, 2, 2, 0, 2 },
        { 2, 2, 1, 2, 4, 2, 1, 2, 2, 1, 0, 2, 1, 1 },
        { 2, 2, 2, 1, 2, 4, 0, 2, 2, 1, 3, 3, 2, 1 },
        { 2, 2, 1, 1, 1, 0, 4, 2, 2, 2, 2, 2, 1, 2 },
        { 1, 2, 3, 1, 2, 2, 2, 4, 3, 0, 3, 2, 2, 1 },
        { 0, 3, 3, 1, 2, 2, 2, 3, 4, 1, 2, 2, 2, 1 },
        { 1, 1, 1, 2, 1, 1, 2, 0, 1, 4, 1, 1, 2, 1 },
        { 3, 2, 2, 2, 0, 3, 2, 3, 2, 1, 4, 2, 2, 1 },
        { 3, 3, 0, 2, 2, 3, 2, 2, 2, 1, 2, 4, 3, 2 },
        { 2, 2, 3, 0, 1, 2, 1, 2, 2, 2, 2, 3, 4, 2 },
        { 1, 0, 1, 2, 1, 1, 2, 1, 1, 1, 1, 2, 2, 4 }
    };

    private static readonly string[] Ganas = ["Deva", "Manushya", "Rakshasa"];

    // Rows: boy's gana, columns: girl's gana.
    private static readonly double[,] GanaTable =
    {
        { 6, 6, 1 },
        { 5, 6, 0 },
        { 1, 0, 6 }
    };

    public static MatchResult Match(KundaliChart boy, KundaliChart girl)
    {
        var bm = boy.Planets.First(p => p.Graha == nameof(Graha.Moon));
        var gm = girl.Planets.First(p => p.Graha == nameof(Graha.Moon));
        var bNak = Vedic.Nakshatras[Vedic.NakshatraOf(bm.Longitude)];
        var gNak = Vedic.Nakshatras[Vedic.NakshatraOf(gm.Longitude)];

        var kootas = new List<KootaScore>();

        // 1. Varna — spiritual compatibility / ego.
        kootas.Add(new KootaScore("Varna", VarnaRank[bm.Rashi] >= VarnaRank[gm.Rashi] ? 1 : 0, 1,
            Varna(bm.Rashi), Varna(gm.Rashi), "Spiritual compatibility and mutual respect."));

        // 2. Vashya — mutual attraction / influence.
        var bv = VashyaIndex(bm.Longitude);
        var gv = VashyaIndex(gm.Longitude);
        kootas.Add(new KootaScore("Vashya", VashyaTable[bv, gv], 2, VashyaNames[bv], VashyaNames[gv], "Mutual attraction and influence over each other."));

        // 3. Tara — destiny / wellbeing, counted both ways between birth stars.
        double TaraFrom(int from, int to)
        {
            var remainder = ((((to - from) + 27) % 27) + 1) % 9;
            return remainder is 3 or 5 or 7 ? 0 : 1.5;
        }

        kootas.Add(new KootaScore("Tara", TaraFrom(gNak.Index, bNak.Index) + TaraFrom(bNak.Index, gNak.Index), 3,
            bNak.Name, gNak.Name, "Health, wellbeing and fortune of the couple."));

        // 4. Yoni — physical and temperamental compatibility.
        var by = Array.IndexOf(Yonis, bNak.Yoni);
        var gy = Array.IndexOf(Yonis, gNak.Yoni);
        kootas.Add(new KootaScore("Yoni", YoniTable[by, gy], 4, bNak.Yoni, gNak.Yoni, "Physical intimacy and temperament."));

        // 5. Graha Maitri — friendship between Moon-sign lords.
        var bLord = Vedic.Rashis[bm.Rashi].Lord;
        var gLord = Vedic.Rashis[gm.Rashi].Lord;
        kootas.Add(new KootaScore("Graha Maitri", MaitriScore(bLord, gLord), 5, bLord.ToString(), gLord.ToString(), "Mental compatibility and friendship."));

        // 6. Gana — temperament.
        var bg = Array.IndexOf(Ganas, bNak.Gana);
        var gg = Array.IndexOf(Ganas, gNak.Gana);
        kootas.Add(new KootaScore("Gana", GanaTable[bg, gg], 6, bNak.Gana, gNak.Gana, "Nature and behaviour towards each other."));

        // 7. Bhakoot — Moon signs' relative position: 2/12, 5/9 and 6/8 are inauspicious.
        var dist = ((bm.Rashi - gm.Rashi + 12) % 12) + 1;
        var bhakootBad = dist is 2 or 12 or 5 or 9 or 6 or 8;
        kootas.Add(new KootaScore("Bhakoot", bhakootBad ? 0 : 7, 7, bm.RashiName, gm.RashiName, "Family welfare, finances and prosperity."));

        // 8. Nadi — health and progeny; same nadi is Nadi dosha.
        kootas.Add(new KootaScore("Nadi", bNak.Nadi == gNak.Nadi ? 0 : 8, 8, bNak.Nadi, gNak.Nadi, "Health and progeny. Same nadi is Nadi dosha."));

        var total = kootas.Sum(k => k.Obtained);
        var verdict = total switch
        {
            >= 32 => "Excellent match",
            >= 25 => "Very good match",
            >= 18 => "Acceptable match",
            _ => "Not recommended"
        };

        var bManglik = boy.Manglik.FromLagna || boy.Manglik.FromMoon;
        var gManglik = girl.Manglik.FromLagna || girl.Manglik.FromMoon;
        var manglikNote = (bManglik, gManglik) switch
        {
            (true, true) => "Both are Manglik, so the dosha is considered cancelled.",
            (false, false) => "Neither is Manglik.",
            (true, false) => $"{boy.Birth.Name} is Manglik and {girl.Birth.Name} is not. Consult an astrologer for remedies.",
            _ => $"{girl.Birth.Name} is Manglik and {boy.Birth.Name} is not. Consult an astrologer for remedies."
        };

        return new MatchResult(
            new MatchPerson(boy.Birth.Name, bm.RashiName, bNak.Name, bm.Pada, boy.Manglik),
            new MatchPerson(girl.Birth.Name, gm.RashiName, gNak.Name, gm.Pada, girl.Manglik),
            kootas, total, verdict, manglikNote);
    }

    private static double MaitriScore(Graha a, Graha b)
    {
        if (a == b) return 5;
        var ab = Vedic.Relationship(a, b);
        var ba = Vedic.Relationship(b, a);
        return (ab, ba) switch
        {
            (Vedic.Friendship.Friend, Vedic.Friendship.Friend) => 5,
            (Vedic.Friendship.Friend, Vedic.Friendship.Neutral) or (Vedic.Friendship.Neutral, Vedic.Friendship.Friend) => 4,
            (Vedic.Friendship.Neutral, Vedic.Friendship.Neutral) => 3,
            (Vedic.Friendship.Friend, Vedic.Friendship.Enemy) or (Vedic.Friendship.Enemy, Vedic.Friendship.Friend) => 1,
            (Vedic.Friendship.Neutral, Vedic.Friendship.Enemy) or (Vedic.Friendship.Enemy, Vedic.Friendship.Neutral) => 0.5,
            _ => 0
        };
    }
}
