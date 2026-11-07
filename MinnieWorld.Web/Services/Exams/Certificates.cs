using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace MinnieWorld.Web.Services.Exams;

/// <summary>
/// Certificates for passed tests. There's no table: the code is the attempt id plus a short hash of details only the
/// server knows (the exact start time), so a code can be checked but not guessed for someone else's attempt.
/// </summary>
public static class Certificates
{
    /// <summary>Same bar as a pass on the result page.</summary>
    public static bool Earned(decimal score, int maxScore) => maxScore > 0 && (double)score >= Endpoints.ExamEndpoints.PassFraction * maxScore;

    /// <summary>"Gold" at 90%+, "Silver" at 75%+, otherwise "Bronze".</summary>
    public static string Level(decimal score, int maxScore)
    {
        var p = maxScore == 0 ? 0 : (double)score / maxScore;
        return p >= 0.9 ? "Gold" : p >= 0.75 ? "Silver" : "Bronze";
    }

    public static string Code(int attemptId, int studentId, DateTime startedAt)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"mw-cert|{attemptId}|{studentId}|{startedAt.Ticks}"));
        return $"MW{attemptId.ToString(CultureInfo.InvariantCulture)}-{Convert.ToHexString(hash, 0, 4)}";
    }

    public static int? AttemptId(string code)
    {
        if (code.Length < 5 || !code.StartsWith("MW", StringComparison.OrdinalIgnoreCase)) return null;
        var dash = code.IndexOf('-');
        return dash > 2 && int.TryParse(code.AsSpan(2, dash - 2), NumberStyles.None, CultureInfo.InvariantCulture, out var id) ? id : null;
    }
}
