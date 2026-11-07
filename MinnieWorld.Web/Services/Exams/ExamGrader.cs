using System.Text.Json;
using MinnieWorld.Web.Models;

namespace MinnieWorld.Web.Services.Exams;

public record SectionScore(string Section, int Total, int Correct, int Wrong, int Unanswered, decimal Marks, decimal MaxMarks);

public static class ExamGrader
{
    /// <summary>Time allowed after the clock runs out for the final autosave or submit to arrive.</summary>
    public static readonly TimeSpan Grace = TimeSpan.FromSeconds(30);

    public static DateTime EndsAt(ExamAttempt attempt, Exam exam) => attempt.StartedAt.AddMinutes(exam.DurationMinutes);

    public static bool IsExpired(ExamAttempt attempt, Exam exam, DateTime nowUtc) => nowUtc > EndsAt(attempt, exam) + Grace;

    public static Dictionary<int, int> ReadAnswers(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>Keeps only answers to this exam's questions with a valid option index.</summary>
    public static string WriteAnswers(Dictionary<int, int?> answers, IReadOnlySet<int> questionIds) =>
        JsonSerializer.Serialize(answers
            .Where(a => a.Value is >= 0 and <= 3 && questionIds.Contains(a.Key))
            .ToDictionary(a => a.Key, a => a.Value!.Value));

    /// <summary>Scores the attempt in place and marks it submitted.</summary>
    public static void Grade(ExamAttempt attempt, Exam exam, IReadOnlyList<Question> questions, DateTime nowUtc)
    {
        var answers = ReadAnswers(attempt.AnswersJson);
        int correct = 0, wrong = 0;
        decimal earned = 0;
        foreach (var q in questions)
        {
            if (!answers.TryGetValue(q.Id, out var chosen)) continue;
            if (chosen == q.Answer)
            {
                correct++;
                earned += q.Marks;
            }
            else
            {
                wrong++;
            }
        }

        attempt.Correct = correct;
        attempt.Wrong = wrong;
        attempt.Unanswered = questions.Count - correct - wrong;
        // Each question carries its own marks (1 unless the paper says otherwise, e.g. JNVST 1.25, AISSEE 2 or 3).
        attempt.MaxScore = (int)Math.Round(questions.Sum(q => q.Marks));
        attempt.Score = Math.Max(0, earned - (wrong * exam.NegativeMark));
        // An attempt finished by the clock is recorded as ending when time ran out, not when someone next looked at it.
        var end = EndsAt(attempt, exam);
        attempt.SubmittedAt = nowUtc < end ? nowUtc : end;
    }

    public static List<SectionScore> Sections(IReadOnlyList<Question> questions, Dictionary<int, int> answers) =>
        questions
            .GroupBy(q => string.IsNullOrWhiteSpace(q.Section) ? "General" : q.Section)
            .Select(g => new SectionScore(
                g.Key,
                g.Count(),
                g.Count(q => answers.TryGetValue(q.Id, out var a) && a == q.Answer),
                g.Count(q => answers.TryGetValue(q.Id, out var a) && a != q.Answer),
                g.Count(q => !answers.ContainsKey(q.Id)),
                g.Where(q => answers.TryGetValue(q.Id, out var a) && a == q.Answer).Sum(q => q.Marks),
                g.Sum(q => q.Marks)))
            .ToList();
}
