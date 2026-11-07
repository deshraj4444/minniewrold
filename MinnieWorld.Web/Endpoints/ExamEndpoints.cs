using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MinnieWorld.Web.Contracts;
using MinnieWorld.Web.Data;
using MinnieWorld.Web.Infrastructure;
using MinnieWorld.Web.Models;
using MinnieWorld.Web.Services.Exams;

namespace MinnieWorld.Web.Endpoints;

/// <summary>Student accounts and online tests on the GK site.</summary>
public static class ExamEndpoints
{
    public const string StudentScheme = "Student";
    public const string StudentPolicy = "Student";
    public const string StampClaim = "mw:stamp";

    /// <summary>
    /// Runs on every request with a student cookie: signs the student out if the account was deleted or disabled,
    /// or its password was reset by an admin (the security stamp changed).
    /// </summary>
    public static async Task ValidateStudentAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieValidatePrincipalContext ctx)
    {
        var id = int.TryParse(ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var v) ? v : 0;
        var stamp = ctx.Principal?.FindFirstValue(StampClaim);
        var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var ok = await db.Students.AnyAsync(s => s.Id == id && !s.IsDisabled && s.SecurityStamp == stamp);
        if (!ok)
        {
            ctx.RejectPrincipal();
            await ctx.HttpContext.SignOutAsync(StudentScheme);
        }
    }

    /// <summary>Score needed to qualify, as a fraction of the maximum (HP TET: 60% for the general category).</summary>
    public const double PassFraction = 0.6;

    public static void MapExamEndpoints(this IEndpointRouteBuilder app)
    {
        MapStudentAuth(app.MapGroup("/api/students").AddEndpointFilter<RequireCsrfHeaderFilter>());
        MapExams(app.MapGroup("/api/exams").AddEndpointFilter<RequireCsrfHeaderFilter>());
        MapAttempts(app.MapGroup("/api/attempts").AddEndpointFilter<RequireCsrfHeaderFilter>().RequireAuthorization(StudentPolicy));
    }

    private static void MapStudentAuth(RouteGroupBuilder g)
    {
        g.MapPost("/register", async (StudentRegisterRequest req, AppDbContext db, IPasswordHasher<Student> hasher, HttpContext http) =>
        {
            var email = req.Email.Trim().ToLowerInvariant();
            if (await db.Students.AnyAsync(s => s.Email == email))
            {
                return Results.Problem("An account with this email already exists. Please log in.", statusCode: StatusCodes.Status409Conflict);
            }

            var student = new Student { FullName = req.FullName.Trim(), Email = email, Phone = req.Phone.Trim(), LastLoginAt = DateTime.UtcNow };
            student.PasswordHash = hasher.HashPassword(student, req.Password);
            db.Students.Add(student);
            await db.SaveChangesAsync();
            await SignInAsync(http, student);
            return Results.Ok(Me(student));
        }).Validate<StudentRegisterRequest>().RequireRateLimiting(RateLimits.Forms);

        g.MapPost("/login", async (LoginRequest req, AppDbContext db, IPasswordHasher<Student> hasher, HttpContext http) =>
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var student = await db.Students.FirstOrDefaultAsync(s => s.Email == email);
            var result = student is null ? PasswordVerificationResult.Failed : hasher.VerifyHashedPassword(student, student.PasswordHash, req.Password);
            if (student is null || result == PasswordVerificationResult.Failed)
            {
                return Results.Problem("Incorrect email or password.", statusCode: StatusCodes.Status401Unauthorized);
            }

            if (student.IsDisabled)
            {
                return Results.Problem("This account has been disabled. Please contact us.", statusCode: StatusCodes.Status403Forbidden);
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                student.PasswordHash = hasher.HashPassword(student, req.Password);
            }

            student.LastLoginAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            await SignInAsync(http, student);
            return Results.Ok(Me(student));
        }).Validate<LoginRequest>().RequireRateLimiting(RateLimits.Login);

        g.MapPost("/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(StudentScheme);
            return Results.NoContent();
        });

        g.MapGet("/me", async (HttpContext http, AppDbContext db) =>
            await CurrentStudentAsync(http, db) is { } s ? Results.Ok(Me(s)) : Results.Unauthorized());

        g.MapGet("/me/attempts", async (HttpContext http, AppDbContext db) =>
        {
            var id = await StudentIdAsync(http);
            if (id is null) return Results.Unauthorized();
            var attempts = await db.ExamAttempts.AsNoTracking()
                .Where(a => a.StudentId == id)
                .OrderByDescending(a => a.StartedAt)
                .Take(100)
                .Select(a => new
                {
                    a.Id, a.StudentId, a.StartedAt, a.SubmittedAt, a.Score, a.MaxScore, a.Correct, a.Wrong, a.Unanswered,
                    Exam = new { a.Exam!.Title, a.Exam.Slug, a.Exam.Category, a.Exam.Kind, a.Exam.DurationMinutes }
                })
                .ToListAsync();
            return Results.Ok(attempts.Select(a => new
            {
                a.Id, a.StartedAt, a.SubmittedAt, a.Score, a.MaxScore, a.Correct, a.Wrong, a.Unanswered, a.Exam,
                Certificate = a.SubmittedAt is not null && Certificates.Earned(a.Score, a.MaxScore) ? Certificates.Code(a.Id, a.StudentId, a.StartedAt) : null
            }));
        });
    }

    private static void MapExams(RouteGroupBuilder g)
    {
        g.MapGet("/", async (AppDbContext db, string? category, string? kind) =>
        {
            var query = db.Exams.AsNoTracking().Where(e => e.IsPublished && e.Questions.Any());
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(e => e.Category == category);
            if (!string.IsNullOrWhiteSpace(kind)) query = query.Where(e => e.Kind == kind);
            return await query
                .OrderBy(e => e.Category).ThenBy(e => e.Kind).ThenByDescending(e => e.Year).ThenBy(e => e.Title)
                .Select(e => new
                {
                    e.Id, e.Title, e.Slug, e.Description, e.Category, e.Kind, e.Year, e.DurationMinutes, e.NegativeMark, e.Price,
                    QuestionCount = e.Questions.Count,
                    MaxMarks = e.Questions.Sum(q => q.Marks),
                    Attempts = db.ExamAttempts.Count(a => a.ExamId == e.Id && a.SubmittedAt != null)
                })
                .ToListAsync();
        });

        // Public: anyone with the code printed on a certificate can check that it is genuine.
        g.MapGet("/certificates/{code}", async (string code, AppDbContext db) =>
        {
            if (Certificates.AttemptId(code) is not { } attemptId) return Results.NotFound();
            var a = await db.ExamAttempts.AsNoTracking().Include(x => x.Exam).Include(x => x.Student)
                .FirstOrDefaultAsync(x => x.Id == attemptId && x.SubmittedAt != null);
            if (a is null || !Certificates.Earned(a.Score, a.MaxScore)
                || !string.Equals(Certificates.Code(a.Id, a.StudentId, a.StartedAt), code, StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound();
            }

            return Results.Ok(new
            {
                Code = Certificates.Code(a.Id, a.StudentId, a.StartedAt),
                StudentName = a.Student!.FullName,
                Exam = new { a.Exam!.Title, a.Exam.Slug, a.Exam.Category, a.Exam.Kind },
                a.Score,
                a.MaxScore,
                Percent = Math.Round(a.Score * 100 / a.MaxScore, 1),
                Level = Certificates.Level(a.Score, a.MaxScore),
                Date = a.SubmittedAt
            });
        });

        // Quick-fire practice game: random questions (with answers) from the free tests of one exam. No login needed.
        g.MapGet("/practice", async (string category, int? count, AppDbContext db) =>
        {
            var n = Math.Clamp(count ?? 10, 5, 30);
            var ids = await db.Questions.AsNoTracking()
                .Where(q => q.Exam!.Category == category && q.Exam.IsPublished && q.Exam.Price == 0)
                .Select(q => q.Id)
                .ToListAsync();
            var pick = ids.OrderBy(_ => Random.Shared.Next()).Take(n).ToHashSet();
            var questions = await db.Questions.AsNoTracking().Where(q => pick.Contains(q.Id)).ToListAsync();
            return Results.Ok(questions.OrderBy(_ => Random.Shared.Next()).Select(q => new
            {
                q.Id, q.Section, q.Text, q.Options, q.TextHi, OptionsHi = HindiOptions(q), q.Answer, q.Explanation, q.ExplanationHi,
                Figure = Blank(q.Figure), Passage = Blank(q.Passage), PassageHi = Blank(q.PassageHi)
            }));
        });

        g.MapGet("/{slug}", async (string slug, AppDbContext db, HttpContext http) =>
        {
            var exam = await db.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Slug == slug && e.IsPublished);
            if (exam is null) return Results.NotFound();

            var sections = await db.Questions.Where(q => q.ExamId == exam.Id)
                .GroupBy(q => q.Section)
                .Select(s => new { Section = s.Key, Count = s.Count(), Marks = s.Sum(q => q.Marks), First = s.Min(q => q.SortOrder) })
                .ToListAsync();

            var studentId = await StudentIdAsync(http);
            var mine = await db.ExamAttempts.AsNoTracking()
                .Where(a => studentId != null && a.ExamId == exam.Id && a.StudentId == studentId)
                .OrderByDescending(a => a.StartedAt)
                .Select(a => new { a.Id, a.StartedAt, a.SubmittedAt, a.Score, a.MaxScore })
                .ToListAsync();

            var submitted = db.ExamAttempts.Where(a => a.ExamId == exam.Id && a.SubmittedAt != null);
            var purchase = await LatestPurchaseAsync(db, exam.Id, studentId);
            var access = await AccessAsync(db, exam, purchase, studentId);
            var settings = await AdminExamEndpoints.SettingsAsync(db);
            return Results.Ok(new
            {
                exam.Id, exam.Title, exam.Slug, exam.Description, exam.Category, exam.Kind, exam.Year, exam.DurationMinutes, exam.NegativeMark, exam.Price,
                Access = access,
                Purchase = purchase is null ? null : new { purchase.Status, purchase.UpiRef, purchase.Note, purchase.CreatedAt },
                Payment = access is "locked" or "pending" or "rejected"
                    ? new
                    {
                        UpiId = settings[SettingKeys.UpiId],
                        PayeeName = settings[SettingKeys.PayeeName],
                        HelpWhatsApp = new string(settings[SettingKeys.PaymentHelpWhatsApp].Where(char.IsDigit).ToArray()),
                        // Goes in the UPI note so the payment is easy to spot in the owner's Google Pay history.
                        Reference = studentId is null ? "" : $"MW{studentId}T{exam.Id}"
                    }
                    : null,
                QuestionCount = sections.Sum(s => s.Count),
                MaxMarks = sections.Sum(s => s.Marks),
                Sections = sections.OrderBy(s => s.First).Select(s => new { s.Section, s.Count, s.Marks }),
                PassPercent = (int)(PassFraction * 100),
                Attempts = await submitted.CountAsync(),
                AverageScore = await submitted.AverageAsync(a => (decimal?)a.Score),
                MyAttempts = mine
            });
        });

        // Starts a new attempt, or resumes the one already running so a refresh or a second tab doesn't restart the clock.
        g.MapPost("/{slug}/attempts", async (string slug, AppDbContext db, HttpContext http) =>
        {
            var studentId = await StudentIdAsync(http);
            if (studentId is null) return Results.Unauthorized();

            var exam = await db.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Slug == slug && e.IsPublished);
            if (exam is null) return Results.NotFound();
            if (!CanStart(await AccessAsync(db, exam, await LatestPurchaseAsync(db, exam.Id, studentId), studentId)))
            {
                return Results.Problem($"This test costs ₹{exam.Price}. Pay and submit your UPI transaction ID to unlock it.", statusCode: StatusCodes.Status402PaymentRequired);
            }

            var now = DateTime.UtcNow;
            var running = await db.ExamAttempts
                .Where(a => a.ExamId == exam.Id && a.StudentId == studentId && a.SubmittedAt == null)
                .OrderByDescending(a => a.StartedAt)
                .ToListAsync();

            var questions = await LoadQuestionsAsync(db, exam.Id);
            if (questions.Count == 0) return Results.BadRequest(new { message = "This test has no questions yet." });

            // Older attempts whose time is up are graded now, so they show up in the history.
            foreach (var old in running.Where(a => ExamGrader.IsExpired(a, exam, now)))
            {
                ExamGrader.Grade(old, exam, questions, now);
            }

            var attempt = running.FirstOrDefault(a => a.SubmittedAt == null);
            if (attempt is null)
            {
                attempt = new ExamAttempt { ExamId = exam.Id, StudentId = studentId.Value, StartedAt = now };
                db.ExamAttempts.Add(attempt);
            }

            await db.SaveChangesAsync();
            return Results.Ok(new { attempt.Id });
        });

        // The student says they've paid by UPI; the admin checks the transaction id and approves it.
        g.MapPost("/{slug}/purchase", async (string slug, PurchaseRequest req, AppDbContext db, HttpContext http) =>
        {
            var studentId = await StudentIdAsync(http);
            if (studentId is null) return Results.Unauthorized();

            var exam = await db.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Slug == slug && e.IsPublished);
            if (exam is null) return Results.NotFound();
            if (exam.Price <= 0) return Results.BadRequest(new { message = "This test is free." });

            var latest = await LatestPurchaseAsync(db, exam.Id, studentId);
            if (latest?.Status == PaymentStatus.Approved) return Results.Ok(new { status = latest.Status });
            if (await AccessAsync(db, exam, latest, studentId) == "all-access") return Results.Ok(new { status = PaymentStatus.Approved });

            var upiRef = req.UpiRef.Trim();
            if (await db.ExamPurchases.AnyAsync(p => p.UpiRef == upiRef && p.Status != PaymentStatus.Rejected && p.Id != (latest == null ? 0 : latest.Id)))
            {
                return Results.Problem("This transaction ID has already been used. Each payment unlocks one test.", statusCode: StatusCodes.Status409Conflict);
            }

            // A pending claim can be corrected (e.g. a mistyped id) without creating a second one.
            if (latest?.Status == PaymentStatus.Pending)
            {
                latest.UpiRef = upiRef;
                latest.CreatedAt = DateTime.UtcNow;
                db.ExamPurchases.Update(latest);
            }
            else
            {
                db.ExamPurchases.Add(new ExamPurchase { StudentId = studentId.Value, ExamId = exam.Id, Amount = exam.Price, UpiRef = upiRef });
            }

            await db.SaveChangesAsync();
            return Results.Ok(new { status = PaymentStatus.Pending });
        }).Validate<PurchaseRequest>().RequireRateLimiting(RateLimits.Forms);
    }

    private static bool CanStart(string access) => access is "free" or "unlocked" or "all-access";

    /// <summary>Like <see cref="Access"/>, but an active all-access pass unlocks every paid test.</summary>
    private static async Task<string> AccessAsync(AppDbContext db, Exam exam, ExamPurchase? purchase, int? studentId)
    {
        var access = Access(exam, purchase);
        if (access is "free" or "unlocked" || studentId is null) return access;
        var now = DateTime.UtcNow;
        var allAccess = await db.Students.AnyAsync(s => s.Id == studentId && s.HasAllAccess && (s.AllAccessUntil == null || s.AllAccessUntil > now));
        return allAccess ? "all-access" : access;
    }

    /// <summary>free, unlocked (paid and approved), pending (waiting for the admin), rejected or locked.</summary>
    private static string Access(Exam exam, ExamPurchase? purchase) =>
        exam.Price <= 0 ? "free"
        : purchase?.Status switch
        {
            PaymentStatus.Approved => "unlocked",
            PaymentStatus.Pending => "pending",
            PaymentStatus.Rejected => "rejected",
            _ => "locked"
        };

    /// <summary>An approved purchase wins over later claims; otherwise the most recent claim.</summary>
    private static async Task<ExamPurchase?> LatestPurchaseAsync(AppDbContext db, int examId, int? studentId) =>
        studentId is null ? null : await db.ExamPurchases.AsNoTracking()
            .Where(p => p.ExamId == examId && p.StudentId == studentId)
            .OrderByDescending(p => p.Status == PaymentStatus.Approved).ThenByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

    private static void MapAttempts(RouteGroupBuilder g)
    {
        g.MapGet("/{id:int}", async (int id, AppDbContext db, HttpContext http) =>
        {
            var (attempt, exam, questions) = await LoadAttemptAsync(db, http, id);
            if (attempt is null) return Results.NotFound();

            var now = DateTime.UtcNow;
            if (attempt.SubmittedAt is null && ExamGrader.IsExpired(attempt, exam!, now))
            {
                ExamGrader.Grade(attempt, exam!, questions, now);
                await db.SaveChangesAsync();
            }

            return attempt.SubmittedAt is null
                ? Results.Ok(InProgress(attempt, exam!, questions, now))
                : Results.Ok(await ResultAsync(db, attempt, exam!, questions));
        });

        g.MapPut("/{id:int}/answers", async (int id, SaveAnswersRequest req, AppDbContext db, HttpContext http) =>
        {
            var (attempt, exam, questions) = await LoadAttemptAsync(db, http, id);
            if (attempt is null) return Results.NotFound();
            if (attempt.SubmittedAt is not null || ExamGrader.IsExpired(attempt, exam!, DateTime.UtcNow))
            {
                return Results.Conflict(new { message = "This test has already finished." });
            }

            attempt.AnswersJson = ExamGrader.WriteAnswers(req.Answers!, questions.Select(q => q.Id).ToHashSet());
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).Validate<SaveAnswersRequest>();

        g.MapPost("/{id:int}/submit", async (int id, SaveAnswersRequest req, AppDbContext db, HttpContext http) =>
        {
            var (attempt, exam, questions) = await LoadAttemptAsync(db, http, id);
            if (attempt is null) return Results.NotFound();

            var now = DateTime.UtcNow;
            if (attempt.SubmittedAt is null)
            {
                // Answers sent after the grace period are ignored; the last autosave counts.
                if (!ExamGrader.IsExpired(attempt, exam!, now))
                {
                    attempt.AnswersJson = ExamGrader.WriteAnswers(req.Answers!, questions.Select(q => q.Id).ToHashSet());
                }

                ExamGrader.Grade(attempt, exam!, questions, now);
                await db.SaveChangesAsync();
            }

            return Results.Ok(await ResultAsync(db, attempt, exam!, questions));
        }).Validate<SaveAnswersRequest>();
    }

    private static object InProgress(ExamAttempt attempt, Exam exam, List<Question> questions, DateTime now) => new
    {
        attempt.Id,
        Status = "in-progress",
        Exam = new { exam.Title, exam.Slug, exam.Category, exam.DurationMinutes, exam.NegativeMark },
        attempt.StartedAt,
        EndsAt = ExamGrader.EndsAt(attempt, exam),
        ServerNow = now,
        Answers = ExamGrader.ReadAnswers(attempt.AnswersJson),
        Questions = questions.Select(q => new { q.Id, q.Section, q.Text, q.Options, q.TextHi, OptionsHi = HindiOptions(q), q.Marks, Figure = Blank(q.Figure), Passage = Blank(q.Passage), PassageHi = Blank(q.PassageHi) })
    };

    private static string? Blank(string s) => string.IsNullOrWhiteSpace(s) ? null : s;

    /// <summary>The Hindi options, or null when the question has no complete Hindi version.</summary>
    private static string[]? HindiOptions(Question q) => q.OptionsHi.All(o => !string.IsNullOrWhiteSpace(o)) ? q.OptionsHi : null;

    private static async Task<object> ResultAsync(AppDbContext db, ExamAttempt attempt, Exam exam, List<Question> questions)
    {
        var answers = ExamGrader.ReadAnswers(attempt.AnswersJson);
        var submitted = db.ExamAttempts.Where(a => a.ExamId == exam.Id && a.SubmittedAt != null);
        var candidates = await submitted.CountAsync();
        var better = await submitted.CountAsync(a => a.Score > attempt.Score);
        var notBetter = await submitted.CountAsync(a => a.Score <= attempt.Score);

        return new
        {
            attempt.Id,
            Status = "submitted",
            Exam = new { exam.Title, exam.Slug, exam.Category, exam.DurationMinutes, exam.NegativeMark },
            attempt.StartedAt,
            attempt.SubmittedAt,
            TimeTakenSeconds = (int)(attempt.SubmittedAt!.Value - attempt.StartedAt).TotalSeconds,
            attempt.Score,
            attempt.MaxScore,
            attempt.Correct,
            attempt.Wrong,
            attempt.Unanswered,
            Percent = attempt.MaxScore == 0 ? 0 : Math.Round(attempt.Score * 100 / attempt.MaxScore, 1),
            PassPercent = (int)(PassFraction * 100),
            Passed = attempt.MaxScore > 0 && (double)attempt.Score >= PassFraction * attempt.MaxScore,
            Certificate = Certificates.Earned(attempt.Score, attempt.MaxScore) ? Certificates.Code(attempt.Id, attempt.StudentId, attempt.StartedAt) : null,
            Level = Certificates.Level(attempt.Score, attempt.MaxScore),
            Rank = better + 1,
            Candidates = candidates,
            Percentile = candidates == 0 ? 100 : Math.Round(notBetter * 100.0 / candidates, 1),
            Sections = ExamGrader.Sections(questions, answers),
            Questions = questions.Select(q => new
            {
                q.Id, q.Section, q.Text, q.Options, q.Answer, q.Explanation, q.TextHi, OptionsHi = HindiOptions(q), q.ExplanationHi,
                q.Marks, Figure = Blank(q.Figure), Passage = Blank(q.Passage), PassageHi = Blank(q.PassageHi),
                Chosen = answers.TryGetValue(q.Id, out var c) ? c : (int?)null
            })
        };
    }

    private static async Task<(ExamAttempt? Attempt, Exam? Exam, List<Question> Questions)> LoadAttemptAsync(AppDbContext db, HttpContext http, int id)
    {
        var studentId = await StudentIdAsync(http);
        var attempt = await db.ExamAttempts.Include(a => a.Exam).FirstOrDefaultAsync(a => a.Id == id && a.StudentId == studentId);
        if (attempt is null) return (null, null, []);
        return (attempt, attempt.Exam, await LoadQuestionsAsync(db, attempt.ExamId));
    }

    private static Task<List<Question>> LoadQuestionsAsync(AppDbContext db, int examId) =>
        db.Questions.AsNoTracking().Where(q => q.ExamId == examId).OrderBy(q => q.SortOrder).ThenBy(q => q.Id).ToListAsync();

    private static object Me(Student s) => new { s.Id, s.FullName, s.Email, s.Phone, AllAccess = s.AllAccessActive(DateTime.UtcNow), s.AllAccessUntil };

    private static async Task SignInAsync(HttpContext http, Student student)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, student.Id.ToString()),
            new Claim(ClaimTypes.Email, student.Email),
            new Claim(ClaimTypes.Name, student.FullName),
            new Claim(StampClaim, student.SecurityStamp)
        ], StudentScheme);
        await http.SignInAsync(StudentScheme, new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = true });
    }

    /// <summary>The signed-in student's id from the student cookie (independent of any admin login).</summary>
    private static async Task<int?> StudentIdAsync(HttpContext http)
    {
        var result = await http.AuthenticateAsync(StudentScheme);
        return result.Succeeded && int.TryParse(result.Principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    }

    private static async Task<Student?> CurrentStudentAsync(HttpContext http, AppDbContext db) =>
        await StudentIdAsync(http) is { } id ? await db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id) : null;
}
