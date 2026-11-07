using System.Globalization;
using System.Text;

namespace MinnieWorld.Web.Infrastructure;

public static class Slug
{
    /// <summary>"Kundali Basics: Houses & Planets" → "kundali-basics-houses-planets". Accents are stripped; non-Latin text falls back to "item".</summary>
    public static string Create(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        var lastWasDash = true;

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                builder.Append(c);
                lastWasDash = false;
            }
            else if (!lastWasDash)
            {
                builder.Append('-');
                lastWasDash = true;
            }
        }

        var slug = builder.ToString().Trim('-');
        if (slug.Length > 200)
        {
            slug = slug[..200].TrimEnd('-');
        }

        return slug.Length == 0 ? "item" : slug;
    }

    /// <summary>Returns <paramref name="baseSlug"/>, or baseSlug-2, -3… until <paramref name="isTaken"/> says it is free.</summary>
    public static async Task<string> MakeUniqueAsync(string baseSlug, Func<string, Task<bool>> isTaken)
    {
        var candidate = baseSlug;
        for (var i = 2; await isTaken(candidate); i++)
        {
            candidate = $"{baseSlug}-{i}";
        }

        return candidate;
    }
}
