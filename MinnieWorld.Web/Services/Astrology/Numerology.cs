namespace MinnieWorld.Web.Services.Astrology;

public record NumberMeaning(int Number, string Planet, string Title, string Description);

public record NumerologyResult(string Name, DateOnly BirthDate, NumberMeaning Moolank, NumberMeaning Bhagyank, NumberMeaning Namank, int[] LuckyNumbers, string[] LuckyColours, string LuckyDay);

/// <summary>Indian (Chaldean) numerology: Moolank from the birth day, Bhagyank from the full date, Namank from the name.</summary>
public static class Numerology
{
    private static readonly Dictionary<char, int> Chaldean = new()
    {
        ['A'] = 1, ['B'] = 2, ['C'] = 3, ['D'] = 4, ['E'] = 5, ['F'] = 8, ['G'] = 3, ['H'] = 5, ['I'] = 1,
        ['J'] = 1, ['K'] = 2, ['L'] = 3, ['M'] = 4, ['N'] = 5, ['O'] = 7, ['P'] = 8, ['Q'] = 1, ['R'] = 2,
        ['S'] = 3, ['T'] = 4, ['U'] = 6, ['V'] = 6, ['W'] = 6, ['X'] = 5, ['Y'] = 1, ['Z'] = 7
    };

    private static readonly NumberMeaning[] Meanings =
    [
        new(1, "Sun", "The Leader", "Independent, ambitious and original. You like to take charge and do best when you set your own direction."),
        new(2, "Moon", "The Peacemaker", "Sensitive, caring and diplomatic. You build harmony and are guided by intuition and emotion."),
        new(3, "Jupiter", "The Teacher", "Wise, optimistic and expressive. Learning, teaching and guiding others come naturally to you."),
        new(4, "Rahu", "The Builder", "Practical, hard-working and unconventional. You build steadily and question established ways."),
        new(5, "Mercury", "The Communicator", "Quick-witted, adaptable and curious. You thrive on change, travel, business and conversation."),
        new(6, "Venus", "The Nurturer", "Loving, artistic and responsible. Home, beauty, comfort and relationships matter deeply to you."),
        new(7, "Ketu", "The Seeker", "Spiritual, analytical and introspective. You look beneath the surface for deeper truths."),
        new(8, "Saturn", "The Achiever", "Disciplined, patient and determined. Success comes through persistence; rewards grow with time."),
        new(9, "Mars", "The Warrior", "Energetic, courageous and compassionate. You fight for causes and finish what you start.")
    ];

    private static readonly Dictionary<int, (int[] Numbers, string[] Colours, string Day)> Lucky = new()
    {
        [1] = ([1, 2, 4], ["Gold", "Orange", "Yellow"], "Sunday"),
        [2] = ([2, 1, 7], ["White", "Cream", "Light green"], "Monday"),
        [3] = ([3, 6, 9], ["Yellow", "Purple", "Pink"], "Thursday"),
        [4] = ([4, 1, 8], ["Blue", "Grey", "Khaki"], "Saturday"),
        [5] = ([5, 1, 6], ["Green", "Light grey", "White"], "Wednesday"),
        [6] = ([6, 3, 9], ["Pink", "Blue", "White"], "Friday"),
        [7] = ([7, 2, 1], ["Light green", "White", "Yellow"], "Monday"),
        [8] = ([8, 4, 6], ["Dark blue", "Black", "Purple"], "Saturday"),
        [9] = ([9, 3, 6], ["Red", "Crimson", "Pink"], "Tuesday")
    };

    public static NumerologyResult Calculate(string name, DateOnly birthDate)
    {
        var moolank = Reduce(birthDate.Day);
        var bhagyank = Reduce(Digits(birthDate.Day) + Digits(birthDate.Month) + Digits(birthDate.Year));
        var nameSum = name.ToUpperInvariant().Where(Chaldean.ContainsKey).Sum(c => Chaldean[c]);
        var namank = nameSum == 0 ? moolank : Reduce(nameSum);
        var lucky = Lucky[moolank];

        return new NumerologyResult(name.Trim(), birthDate, Meanings[moolank - 1], Meanings[bhagyank - 1], Meanings[namank - 1], lucky.Numbers, lucky.Colours, lucky.Day);
    }

    private static int Digits(int n) => n.ToString().Sum(c => c - '0');

    private static int Reduce(int n)
    {
        while (n > 9) n = Digits(n);
        return n;
    }
}
