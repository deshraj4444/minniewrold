using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MinnieWorld.Web.Contracts;
using MinnieWorld.Web.Data;
using MinnieWorld.Web.Infrastructure;
using MinnieWorld.Web.Models;
using MinnieWorld.Web.Services.Exams;

namespace MinnieWorld.Web.Endpoints;

/// <summary>Admin API for tests, questions, students and site settings. Mapped inside the authorised /api/admin group.</summary>
public static class AdminExamEndpoints
{
    public static void MapAdminExams(this RouteGroupBuilder admin)
    {
        var exams = admin.MapGroup("/exams");

        exams.MapGet("/", async (AppDbContext db) =>
            await db.Exams.AsNoTracking().OrderBy(e => e.Category).ThenBy(e => e.Kind).ThenBy(e => e.Title)
                .Select(e => new
                {
                    e.Id, e.Title, e.Slug, e.Category, e.Kind, e.Year, e.DurationMinutes, e.Price, e.IsPublished, e.UpdatedAt,
                    QuestionCount = e.Questions.Count,
                    Attempts = db.ExamAttempts.Count(a => a.ExamId == e.Id && a.SubmittedAt != null)
                })
                .ToListAsync());

        exams.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var exam = await db.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            if (exam is null) return Results.NotFound();
            var questions = await db.Questions.AsNoTracking().Where(q => q.ExamId == id).OrderBy(q => q.SortOrder).ThenBy(q => q.Id)
                .Select(q => new { q.Id, q.SortOrder, q.Section, q.Text, Options = new[] { q.OptionA, q.OptionB, q.OptionC, q.OptionD }, q.Answer, q.Explanation,
                    q.TextHi, OptionsHi = new[] { q.OptionAHi, q.OptionBHi, q.OptionCHi, q.OptionDHi }, q.ExplanationHi, q.Marks, q.Figure, q.Passage, q.PassageHi })
                .ToListAsync();
            return Results.Ok(new { exam, questions });
        });

        exams.MapPost("/", async (ExamRequest req, AppDbContext db) =>
        {
            var exam = new Exam();
            await ApplyAsync(exam, req, db);
            db.Exams.Add(exam);
            await db.SaveChangesAsync();
            return Results.Ok(exam);
        }).Validate<ExamRequest>();

        exams.MapPut("/{id:int}", async (int id, ExamRequest req, AppDbContext db) =>
        {
            var exam = await db.Exams.FindAsync(id);
            if (exam is null) return Results.NotFound();
            await ApplyAsync(exam, req, db);
            await db.SaveChangesAsync();
            return Results.Ok(exam);
        }).Validate<ExamRequest>();

        exams.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
            await db.Exams.Where(e => e.Id == id).ExecuteDeleteAsync() == 0 ? Results.NotFound() : Results.NoContent());

        exams.MapPost("/{id:int}/questions", async (int id, QuestionRequest req, AppDbContext db) =>
        {
            var exam = await db.Exams.FindAsync(id);
            if (exam is null) return Results.NotFound();
            var q = new Question { ExamId = id, SortOrder = (await db.Questions.Where(x => x.ExamId == id).MaxAsync(x => (int?)x.SortOrder) ?? 0) + 1 };
            Apply(q, req);
            db.Questions.Add(q);
            exam.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(new { q.Id });
        }).Validate<QuestionRequest>();

        exams.MapPost("/{id:int}/import", async (int id, ImportQuestionsRequest req, AppDbContext db) =>
        {
            var exam = await db.Exams.FindAsync(id);
            if (exam is null) return Results.NotFound();

            var parsed = ExamImport.Parse(req.Json);
            if (parsed.Questions.Count == 0)
            {
                return Results.BadRequest(new { message = parsed.Errors.FirstOrDefault() ?? "No questions found.", errors = new { json = parsed.Errors.Take(20) } });
            }

            if (req.Replace)
            {
                await db.Questions.Where(q => q.ExamId == id).ExecuteDeleteAsync();
            }

            var next = req.Replace ? 1 : (await db.Questions.Where(q => q.ExamId == id).MaxAsync(q => (int?)q.SortOrder) ?? 0) + 1;
            foreach (var q in parsed.Questions)
            {
                q.ExamId = id;
                q.SortOrder = next++;
                db.Questions.Add(q);
            }

            exam.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(new { imported = parsed.Questions.Count, skipped = parsed.Errors.Count, errors = parsed.Errors.Take(20) });
        }).Validate<ImportQuestionsRequest>();

        var questions = admin.MapGroup("/questions");

        questions.MapPut("/{id:int}", async (int id, QuestionRequest req, AppDbContext db) =>
        {
            var q = await db.Questions.FindAsync(id);
            if (q is null) return Results.NotFound();
            Apply(q, req);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).Validate<QuestionRequest>();

        questions.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
            await db.Questions.Where(q => q.Id == id).ExecuteDeleteAsync() == 0 ? Results.NotFound() : Results.NoContent());

        MapStudents(admin.MapGroup("/students"));

        var payments = admin.MapGroup("/payments");

        payments.MapGet("/", async (AppDbContext db, string? status) =>
            await db.ExamPurchases.AsNoTracking()
                .Where(p => (string.IsNullOrEmpty(status) || p.Status == status) && !p.UpiRef.StartsWith(GrantPrefix))
                .OrderBy(p => p.Status == PaymentStatus.Pending ? 0 : 1).ThenByDescending(p => p.CreatedAt)
                .Take(300)
                .Select(p => new
                {
                    p.Id, p.Amount, p.UpiRef, p.Status, p.Note, p.CreatedAt, p.ReviewedAt,
                    Student = new { p.Student!.FullName, p.Student.Email, p.Student.Phone },
                    Exam = new { p.Exam!.Title, p.Exam.Slug }
                })
                .ToListAsync());

        payments.MapPut("/{id:int}/approve", async (int id, AppDbContext db) =>
        {
            var p = await db.ExamPurchases.FindAsync(id);
            if (p is null) return Results.NotFound();
            p.Status = PaymentStatus.Approved;
            p.Note = "";
            p.ReviewedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        payments.MapPut("/{id:int}/reject", async (int id, RejectPaymentRequest req, AppDbContext db) =>
        {
            var p = await db.ExamPurchases.FindAsync(id);
            if (p is null) return Results.NotFound();
            p.Status = PaymentStatus.Rejected;
            p.Note = string.IsNullOrWhiteSpace(req.Note) ? "We couldn't find this payment. Please check the transaction ID and try again." : req.Note.Trim();
            p.ReviewedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).Validate<RejectPaymentRequest>();

        admin.MapGet("/settings", async (AppDbContext db) => await SettingsAsync(db));

        admin.MapPut("/settings", async (SettingsRequest req, AppDbContext db) =>
        {
            foreach (var (key, value) in req.Values!.Where(kv => SettingKeys.All.Contains(kv.Key)))
            {
                var trimmed = value.Trim();
                if (trimmed.Length > 2000) return Results.BadRequest(new { message = $"{key} is too long." });
                var row = await db.Settings.FindAsync(key);
                if (row is null) db.Settings.Add(new SiteSetting { Key = key, Value = trimmed });
                else row.Value = trimmed;
            }

            await db.SaveChangesAsync();
            return Results.Ok(await SettingsAsync(db));
        }).Validate<SettingsRequest>();
    }

    private static void MapStudents(RouteGroupBuilder g)
    {
        g.MapGet("/", async (AppDbContext db) =>
            await db.Students.AsNoTracking().OrderByDescending(s => s.CreatedAt).Take(1000)
                .Select(s => new
                {
                    s.Id, s.FullName, s.Email, s.Phone, s.CreatedAt, s.LastLoginAt, s.HasAllAccess, s.AllAccessUntil, s.IsDisabled,
                    Attempts = db.ExamAttempts.Count(a => a.StudentId == s.Id && a.SubmittedAt != null),
                    UnlockedTests = db.ExamPurchases.Count(p => p.StudentId == s.Id && p.Status == PaymentStatus.Approved),
                    BestPercent = db.ExamAttempts.Where(a => a.StudentId == s.Id && a.SubmittedAt != null && a.MaxScore > 0)
                        .Max(a => (decimal?)(a.Score * 100 / a.MaxScore))
                })
                .ToListAsync());

        g.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var s = await db.Students.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (s is null) return Results.NotFound();

            // Every paid test, with whether this student has it unlocked (paid, granted, waiting or rejected).
            var purchases = await db.ExamPurchases.AsNoTracking().Where(p => p.StudentId == id).ToListAsync();
            var tests = await db.Exams.AsNoTracking().Where(e => e.Price > 0).OrderBy(e => e.Kind).ThenBy(e => e.Title)
                .Select(e => new { e.Id, e.Title, e.Price }).ToListAsync();
            var attempts = await db.ExamAttempts.AsNoTracking().Where(a => a.StudentId == id).OrderByDescending(a => a.StartedAt).Take(50)
                .Select(a => new { a.Id, a.StartedAt, a.SubmittedAt, a.Score, a.MaxScore, Exam = a.Exam!.Title }).ToListAsync();

            return Results.Ok(new
            {
                s.Id, s.FullName, s.Email, s.Phone, s.CreatedAt, s.LastLoginAt, s.HasAllAccess, s.AllAccessUntil, s.IsDisabled,
                Tests = tests.Select(t =>
                {
                    var p = purchases.Where(x => x.ExamId == t.Id)
                        .OrderByDescending(x => x.Status == PaymentStatus.Approved).ThenByDescending(x => x.CreatedAt).FirstOrDefault();
                    return new { t.Id, t.Title, t.Price, Status = p?.Status ?? "Locked", Granted = p?.UpiRef.StartsWith(GrantPrefix) == true };
                }),
                Attempts = attempts
            });
        });

        g.MapPost("/", async (AdminStudentRequest req, AppDbContext db, IPasswordHasher<Student> hasher) =>
        {
            var email = req.Email.Trim().ToLowerInvariant();
            if (req.Password.Length < 8) return Results.BadRequest(new { message = "Set a password of at least 8 characters." });
            if (await db.Students.AnyAsync(s => s.Email == email)) return Results.Conflict(new { message = "A student with this email already exists." });

            var student = new Student { Email = email };
            Apply(student, req);
            student.PasswordHash = hasher.HashPassword(student, req.Password);
            db.Students.Add(student);
            await db.SaveChangesAsync();
            return Results.Ok(new { student.Id });
        }).Validate<AdminStudentRequest>();

        g.MapPut("/{id:int}", async (int id, AdminStudentRequest req, AppDbContext db, IPasswordHasher<Student> hasher) =>
        {
            var student = await db.Students.FindAsync(id);
            if (student is null) return Results.NotFound();

            var email = req.Email.Trim().ToLowerInvariant();
            if (email != student.Email && await db.Students.AnyAsync(s => s.Email == email && s.Id != id))
            {
                return Results.Conflict(new { message = "Another student already uses this email." });
            }

            var wasDisabled = student.IsDisabled;
            student.Email = email;
            Apply(student, req);
            if (!string.IsNullOrEmpty(req.Password))
            {
                if (req.Password.Length < 8) return Results.BadRequest(new { message = "The new password must be at least 8 characters." });
                student.PasswordHash = hasher.HashPassword(student, req.Password);
            }

            // A new password or disabling the account signs the student out everywhere.
            if (!string.IsNullOrEmpty(req.Password) || (student.IsDisabled && !wasDisabled))
            {
                student.SecurityStamp = Guid.NewGuid().ToString("N");
            }

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).Validate<AdminStudentRequest>();

        g.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
            await db.Students.Where(s => s.Id == id).ExecuteDeleteAsync() == 0 ? Results.NotFound() : Results.NoContent());

        // Unlock one paid test for a student without a payment.
        g.MapPost("/{id:int}/grants", async (int id, GrantExamRequest req, AppDbContext db) =>
        {
            if (!await db.Students.AnyAsync(s => s.Id == id) || !await db.Exams.AnyAsync(e => e.Id == req.ExamId)) return Results.NotFound();
            if (await db.ExamPurchases.AnyAsync(p => p.StudentId == id && p.ExamId == req.ExamId && p.Status == PaymentStatus.Approved))
            {
                return Results.NoContent();
            }

            db.ExamPurchases.Add(new ExamPurchase
            {
                StudentId = id,
                ExamId = req.ExamId,
                Amount = 0,
                UpiRef = $"{GrantPrefix}{id}-{req.ExamId}-{DateTime.UtcNow:yyMMddHHmmss}",
                Status = PaymentStatus.Approved,
                Note = "Granted by admin",
                ReviewedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).Validate<GrantExamRequest>();

        // Take back a test the admin granted (paid unlocks are left alone; reject those under Payments).
        g.MapDelete("/{id:int}/grants/{examId:int}", async (int id, int examId, AppDbContext db) =>
        {
            await db.ExamPurchases.Where(p => p.StudentId == id && p.ExamId == examId && p.UpiRef.StartsWith(GrantPrefix)).ExecuteDeleteAsync();
            return Results.NoContent();
        });

        static void Apply(Student s, AdminStudentRequest r)
        {
            s.FullName = r.FullName.Trim();
            s.Phone = r.Phone.Trim();
            s.HasAllAccess = r.HasAllAccess;
            s.AllAccessUntil = r.HasAllAccess && r.AllAccessUntil is { } until ? DateTime.SpecifyKind(until, DateTimeKind.Utc) : null;
            s.IsDisabled = r.IsDisabled;
        }
    }

    /// <summary>Marks unlocks an admin gave for free, so they can be told apart from (and revoked separately to) real payments.</summary>
    private const string GrantPrefix = "ADMIN-";

    public static async Task<Dictionary<string, string>> SettingsAsync(AppDbContext db)
    {
        var stored = await db.Settings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value);
        return SettingKeys.All.ToDictionary(k => k, k => stored.TryGetValue(k, out var v) ? v : SettingKeys.Defaults.GetValueOrDefault(k, ""));
    }

    private static async Task ApplyAsync(Exam e, ExamRequest r, AppDbContext db)
    {
        var baseSlug = Slug.Create(string.IsNullOrWhiteSpace(r.Slug) ? r.Title : r.Slug);
        e.Slug = await Slug.MakeUniqueAsync(baseSlug, s => db.Exams.AnyAsync(x => x.Slug == s && x.Id != e.Id));
        e.Title = r.Title.Trim();
        e.Description = r.Description.Trim();
        e.Category = r.Category.Trim();
        e.Kind = r.Kind;
        e.Year = r.Year;
        e.DurationMinutes = r.DurationMinutes;
        e.NegativeMark = r.NegativeMark;
        e.Price = r.Price;
        e.IsPublished = r.IsPublished;
        e.UpdatedAt = DateTime.UtcNow;
    }

    private static void Apply(Question q, QuestionRequest r)
    {
        q.Section = r.Section.Trim();
        q.Text = r.Text.Trim();
        q.OptionA = r.Options[0].Trim();
        q.OptionB = r.Options[1].Trim();
        q.OptionC = r.Options[2].Trim();
        q.OptionD = r.Options[3].Trim();
        q.Answer = r.Answer;
        q.Explanation = r.Explanation.Trim();
        q.TextHi = r.TextHi.Trim();
        var hi = r.OptionsHi.Length == 4 ? r.OptionsHi : ["", "", "", ""];
        q.OptionAHi = (hi[0] ?? "").Trim();
        q.OptionBHi = (hi[1] ?? "").Trim();
        q.OptionCHi = (hi[2] ?? "").Trim();
        q.OptionDHi = (hi[3] ?? "").Trim();
        q.ExplanationHi = r.ExplanationHi.Trim();
        if (r.Marks is { } marks) q.Marks = marks;
        if (r.Figure is not null) q.Figure = r.Figure.Trim();
        if (r.Passage is not null) q.Passage = r.Passage.Trim();
        if (r.PassageHi is not null) q.PassageHi = r.PassageHi.Trim();
    }
}
