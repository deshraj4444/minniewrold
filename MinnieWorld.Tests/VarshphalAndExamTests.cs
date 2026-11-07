using MinnieWorld.Web.Models;
using MinnieWorld.Web.Services.Astrology;
using MinnieWorld.Web.Services.Exams;

namespace MinnieWorld.Tests;

public class VarshphalTests
{
    private static readonly BirthDetails Birth = new("Test", new DateOnly(1990, 10, 22), new TimeOnly(10, 30), 31.1048, 77.1734, "Asia/Kolkata", "Shimla");

    [Fact]
    public void Varsha_pravesh_puts_the_Sun_back_at_its_birth_longitude()
    {
        var birthJd = Ephemeris.JulianDay(PlaceService.ToUtc(Birth.Date, Birth.Time, Birth.TimeZone));
        var natalSun = Ephemeris.SiderealLongitude(Graha.Sun, birthJd);

        var chart = VarshphalCalculator.Calculate(Birth, 2026);
        var returnSun = Ephemeris.SiderealLongitude(Graha.Sun, Ephemeris.JulianDay(chart.VarshaPraveshUtc));

        Assert.True(Math.Abs(returnSun - natalSun) < 0.001, $"Sun at return {returnSun:F5}°, natal {natalSun:F5}°");
        // The sidereal solar return drifts later through the calendar but stays within a few days of the birthday.
        Assert.InRange((chart.VarshaPravesh.Date - new DateTime(2026, 10, 22)).TotalDays, -3, 3);
        Assert.Equal(36, chart.Age);
    }

    [Fact]
    public void Muntha_advances_one_sign_per_year_from_the_birth_lagna()
    {
        var a = VarshphalCalculator.Calculate(Birth, 2025);
        var b = VarshphalCalculator.Calculate(Birth, 2026);
        Assert.Equal((a.Muntha.Rashi + 1) % 12, b.Muntha.Rashi);
        Assert.Equal(5, a.OfficeBearers.Count);
        Assert.Contains(a.YearLord, a.OfficeBearers.Select(o => o.Graha));
    }

    [Fact]
    public void Mudda_dasha_of_the_birth_year_starts_with_the_birth_nakshatra_lord_and_fills_the_year()
    {
        // Ashwini (lord Ketu), age 0 → Ketu first. Bharani (Venus), age 0 → Venus first.
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(365.2564);
        var ashwini = VarshphalCalculator.MuddaDashas(1.0, 0, start, end, TimeZoneInfo.Utc);
        var bharani = VarshphalCalculator.MuddaDashas(14.0, 0, start, end, TimeZoneInfo.Utc);

        Assert.Equal("Ketu", ashwini[0].Lord);
        Assert.Equal("Venus", bharani[0].Lord);
        Assert.Equal(9, ashwini.Count);
        Assert.Equal(end, ashwini[^1].End);
        // Each completed year moves the first lord one step on: age 1 from Ashwini → Venus.
        Assert.Equal("Venus", VarshphalCalculator.MuddaDashas(1.0, 1, start, end, TimeZoneInfo.Utc)[0].Lord);
    }

    [Fact]
    public void Kundali_includes_sade_sati_periods_about_seven_and_a_half_years_long()
    {
        var chart = KundaliCalculator.Calculate(Birth, new DateTime(2026, 10, 23, 0, 0, 0, DateTimeKind.Utc));
        var periods = chart.Insights.SadeSati.Periods;
        Assert.NotEmpty(periods);
        // Saturn takes ~29.5 years per round, so 100 years hold three or four Sade Satis.
        Assert.InRange(periods.Count, 3, 4);
        foreach (var p in periods.Where(p => p.End < new DateOnly(2090, 1, 1) && p.Start > Birth.Date))
        {
            Assert.InRange((p.End.DayNumber - p.Start.DayNumber) / 365.25, 6.5, 8.5);
        }

        Assert.Equal(12, chart.Insights.DashamsaChart.Count);
        Assert.Equal(9, chart.Insights.Transits.Count);
    }
}

public class ExamTests
{
    [Fact]
    public void Import_accepts_letters_option_text_numbers_and_separate_option_fields()
    {
        const string json = """
        {
          "questions": [
            { "question": "Q1", "options": ["(A) a", "(B) b", "(C) c", "(D) d"], "answer": "C", "subject": "English" },
            { "text": "Q2", "options": ["w", "x", "y", "z"], "answer": "y", "explanation": "because" },
            { "q": "Q3", "a": "1", "b": "2", "c": "3", "d": "4", "correct": 3 },
            { "q": "Q4", "option_a": "p", "option_b": "q", "option_c": "r", "option_d": "s", "answer": "2" },
            { "q": "Bad", "options": ["only", "three", "options"], "answer": 0 }
          ]
        }
        """;

        var result = ExamImport.Parse(json);

        Assert.Equal(4, result.Questions.Count);
        Assert.Single(result.Errors);
        Assert.Equal(2, result.Questions[0].Answer);
        Assert.Equal("a", result.Questions[0].OptionA);
        Assert.Equal("English", result.Questions[0].Section);
        Assert.Equal(2, result.Questions[1].Answer);
        Assert.Equal("because", result.Questions[1].Explanation);
        Assert.Equal(3, result.Questions[2].Answer);
        Assert.Equal(1, result.Questions[3].Answer); // "2" as text means the second option
    }

    [Fact]
    public void Grading_counts_correct_wrong_and_skipped_and_applies_negative_marks()
    {
        var exam = new Exam { DurationMinutes = 10, NegativeMark = 0.25m };
        var questions = Enumerable.Range(1, 4).Select(i => new Question { Id = i, Answer = 0, Section = i <= 2 ? "A" : "B" }).ToList();
        var start = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var attempt = new ExamAttempt
        {
            StartedAt = start,
            AnswersJson = ExamGrader.WriteAnswers(new Dictionary<int, int?> { [1] = 0, [2] = 0, [3] = 2, [99] = 0, [4] = null }, questions.Select(q => q.Id).ToHashSet())
        };

        ExamGrader.Grade(attempt, exam, questions, start.AddMinutes(5));

        Assert.Equal(2, attempt.Correct);
        Assert.Equal(1, attempt.Wrong);
        Assert.Equal(1, attempt.Unanswered);
        Assert.Equal(1.75m, attempt.Score);
        Assert.Equal(start.AddMinutes(5), attempt.SubmittedAt);

        var sections = ExamGrader.Sections(questions, ExamGrader.ReadAnswers(attempt.AnswersJson));
        Assert.Equal(["A", "B"], sections.Select(s => s.Section));
        Assert.Equal(2, sections[0].Correct);
        Assert.Equal(1, sections[1].Wrong);
    }

    [Fact]
    public void A_late_attempt_is_recorded_as_ending_when_time_ran_out()
    {
        var exam = new Exam { DurationMinutes = 10 };
        var start = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var attempt = new ExamAttempt { StartedAt = start };

        Assert.True(ExamGrader.IsExpired(attempt, exam, start.AddMinutes(11)));
        ExamGrader.Grade(attempt, exam, [new Question { Id = 1 }], start.AddHours(3));
        Assert.Equal(start.AddMinutes(10), attempt.SubmittedAt);
    }

    [Fact]
    public void Import_reads_the_hindi_version()
    {
        var json = """
        [
          { "text": "Persona means", "textHi": "पर्सोना का अर्थ है", "options": ["Soul", "Mask", "Self", "Role"],
            "optionsHi": ["आत्मा", "मुखौटा", "स्वयं", "भूमिका"], "answer": 1, "explanation": "x", "explanationHi": "य" },
          { "text": "English only", "options": ["a", "b", "c", "d"], "answer": "A" }
        ]
        """;

        var result = ExamImport.Parse(json);

        Assert.Empty(result.Errors);
        Assert.Equal("पर्सोना का अर्थ है", result.Questions[0].TextHi);
        Assert.Equal("मुखौटा", result.Questions[0].OptionBHi);
        Assert.Equal("य", result.Questions[0].ExplanationHi);
        Assert.All(result.Questions[1].OptionsHi, o => Assert.Equal("", o));
    }

    [Fact]
    public void Import_reads_marks_figure_and_passage()
    {
        const string json = """
            [{ "text": "Odd one out", "options": ["Figure (A)", "Figure (B)", "Figure (C)", "Figure (D)"], "answer": 2, "marks": 1.25,
               "figure": "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 10 10\"></svg>", "passage": "P", "passageHi": "प" },
             { "text": "No extras", "options": ["a", "b", "c", "d"], "answer": 0 }]
            """;
        var result = ExamImport.Parse(json);
        Assert.Empty(result.Errors);
        Assert.Equal(1.25m, result.Questions[0].Marks);
        Assert.StartsWith("<svg", result.Questions[0].Figure);
        Assert.Equal("P", result.Questions[0].Passage);
        Assert.Equal("प", result.Questions[0].PassageHi);
        Assert.Equal(1m, result.Questions[1].Marks);
        Assert.Equal("", result.Questions[1].Figure);
    }

    [Fact]
    public void Grading_uses_each_questions_marks_like_aissee()
    {
        var start = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var exam = new Exam { DurationMinutes = 10 };
        List<Question> questions = [new() { Id = 1, Answer = 0, Marks = 3, Section = "Mathematics" }, new() { Id = 2, Answer = 1, Marks = 2, Section = "English" }, new() { Id = 3, Answer = 1, Marks = 2, Section = "English" }];
        var attempt = new ExamAttempt { StartedAt = start, AnswersJson = """{"1":0,"2":1,"3":0}""" };
        ExamGrader.Grade(attempt, exam, questions, start.AddMinutes(5));
        Assert.Equal(7, attempt.MaxScore);
        Assert.Equal(5m, attempt.Score);
        var english = ExamGrader.Sections(questions, ExamGrader.ReadAnswers(attempt.AnswersJson)).Single(s => s.Section == "English");
        Assert.Equal(2m, english.Marks);
        Assert.Equal(4m, english.MaxMarks);
    }

    [Fact]
    public void Certificate_codes_round_trip_and_depend_on_the_attempt()
    {
        var start = new DateTime(2026, 11, 6, 9, 30, 15, DateTimeKind.Utc);
        var code = Certificates.Code(42, 7, start);
        Assert.Equal(42, Certificates.AttemptId(code));
        Assert.NotEqual(code, Certificates.Code(42, 8, start));
        Assert.NotEqual(code, Certificates.Code(42, 7, start.AddTicks(1)));
        Assert.Null(Certificates.AttemptId("hello"));
        Assert.True(Certificates.Earned(60, 100));
        Assert.False(Certificates.Earned(59.75m, 100));
        Assert.Equal("Gold", Certificates.Level(90, 100));
        Assert.Equal("Silver", Certificates.Level(75, 100));
        Assert.Equal("Bronze", Certificates.Level(60, 100));
    }
}
