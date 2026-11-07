namespace MinnieWorld.Web.Models;

/// <summary>A registered learner on the GK site. Separate from <see cref="AdminUser"/> and signed in with its own cookie.</summary>
public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    /// <summary>Unlocks every paid test without payment (set by an admin).</summary>
    public bool HasAllAccess { get; set; }
    /// <summary>When the all-access pass ends (UTC); null = no end date.</summary>
    public DateTime? AllAccessUntil { get; set; }
    /// <summary>A disabled student can't log in, and existing sessions stop working.</summary>
    public bool IsDisabled { get; set; }
    /// <summary>Changes when the password is reset or the account is disabled, which signs out existing sessions.</summary>
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");

    public bool AllAccessActive(DateTime nowUtc) => HasAllAccess && (AllAccessUntil is null || AllAccessUntil > nowUtc);
}

public static class ExamKinds
{
    public const string Mock = "Mock test";
    public const string PreviousYear = "Previous year";
    public const string Sectional = "Sectional";
    public const string Sample = "Sample test";
    /// <summary>An original paper that follows one year's pattern and topic mix (not the official paper).</summary>
    public const string YearPattern = "Year pattern";
    /// <summary>An original paper for the next exam, on the latest announced pattern.</summary>
    public const string Expected = "Expected paper";

    public static readonly string[] All = [Mock, PreviousYear, Sectional, Sample, YearPattern, Expected];
}

/// <summary>A timed test: a full mock, a previous-year paper or a short sectional test.</summary>
public class Exam
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    /// <summary>The exam it prepares for, e.g. "HP TET TGT Arts". Tests are grouped by it.</summary>
    public string Category { get; set; } = string.Empty;
    /// <summary>One of <see cref="ExamKinds"/>.</summary>
    public string Kind { get; set; } = ExamKinds.Mock;
    public int? Year { get; set; }
    public int DurationMinutes { get; set; } = 60;
    /// <summary>Marks deducted per wrong answer (0 = no negative marking, as in HP TET).</summary>
    public decimal NegativeMark { get; set; }
    /// <summary>Price in rupees to unlock the test; 0 = free.</summary>
    public int Price { get; set; }
    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>The "version" of the seed file this test came from; a higher version in the file refreshes the test on start-up.</summary>
    public int SeedVersion { get; set; } = 1;

    public List<Question> Questions { get; set; } = [];
}

public class Question
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public Exam? Exam { get; set; }
    public int SortOrder { get; set; }
    public string Section { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    /// <summary>0–3 for A–D.</summary>
    public int Answer { get; set; }
    public string Explanation { get; set; } = string.Empty;

    // Hindi version, shown under the English like the printed HP TET / CTET papers. Empty for
    // language sections (English questions stay English-only; Hindi questions are written in Text).
    public string TextHi { get; set; } = string.Empty;
    public string OptionAHi { get; set; } = string.Empty;
    public string OptionBHi { get; set; } = string.Empty;
    public string OptionCHi { get; set; } = string.Empty;
    public string OptionDHi { get; set; } = string.Empty;
    public string ExplanationHi { get; set; } = string.Empty;

    /// <summary>Marks for a right answer (JNVST 1.25; AISSEE 2 or 3 by section).</summary>
    public decimal Marks { get; set; } = 1;
    /// <summary>Optional SVG picture (problem and answer figures for non-verbal reasoning). Shown as an image, so it can't run scripts.</summary>
    public string Figure { get; set; } = string.Empty;
    /// <summary>Optional reading passage shown above the question; questions on the same passage repeat it.</summary>
    public string Passage { get; set; } = string.Empty;
    public string PassageHi { get; set; } = string.Empty;

    public string[] Options => [OptionA, OptionB, OptionC, OptionD];
    public string[] OptionsHi => [OptionAHi, OptionBHi, OptionCHi, OptionDHi];
}

/// <summary>One sitting of an exam. The clock runs on the server from <see cref="StartedAt"/>, so refreshing the page doesn't reset it.</summary>
public class ExamAttempt
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public int ExamId { get; set; }
    public Exam? Exam { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    /// <summary>JSON object of question id → chosen option (0–3). Saved as the student answers, so a lost connection loses nothing.</summary>
    public string AnswersJson { get; set; } = "{}";
    public int Correct { get; set; }
    public int Wrong { get; set; }
    public int Unanswered { get; set; }
    public decimal Score { get; set; }
    public int MaxScore { get; set; }
}

public static class PaymentStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}

/// <summary>
/// A student's claim to have paid for a test by UPI to the site owner's personal account. There's no payment
/// gateway, so an admin checks the UPI transaction id against their Google Pay history and approves it.
/// An approved purchase unlocks the test (with retakes) for that student.
/// </summary>
public class ExamPurchase
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public int ExamId { get; set; }
    public Exam? Exam { get; set; }
    public int Amount { get; set; }
    /// <summary>The 12-digit UPI transaction id (UTR) the student copied from their payment app.</summary>
    public string UpiRef { get; set; } = string.Empty;
    public string Status { get; set; } = PaymentStatus.Pending;
    /// <summary>Shown to the student when a payment is rejected.</summary>
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}

/// <summary>A named running total, e.g. the site's visitor count. Incremented with a single UPDATE so concurrent visits don't lose counts.</summary>
public class SiteCounter
{
    public const string Visitors = "visitors";

    public string Key { get; set; } = string.Empty;
    public long Value { get; set; }
}

/// <summary>Simple key/value settings editable in admin (e.g. the astrologer's WhatsApp number).</summary>
public class SiteSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public static class SettingKeys
{
    public const string WhatsAppNumber = "astro.whatsappNumber";
    public const string WhatsAppGreeting = "astro.whatsappGreeting";
    public const string AstrologerName = "astro.astrologerName";
    public const string UpiId = "payments.upiId";
    public const string PayeeName = "payments.payeeName";
    public const string PaymentHelpWhatsApp = "payments.helpWhatsApp";
    /// <summary>The "write a review" link of the Google Business Profile, shown as a "Rate us on Google" button when set.</summary>
    public const string GoogleReviewUrl = "site.googleReviewUrl";

    public static readonly string[] All = [WhatsAppNumber, WhatsAppGreeting, AstrologerName, UpiId, PayeeName, PaymentHelpWhatsApp, GoogleReviewUrl];

    /// <summary>Used until an admin saves a value. Students message this number on WhatsApp to buy tests (messages only, no calls).</summary>
    public static readonly Dictionary<string, string> Defaults = new() { [PaymentHelpWhatsApp] = "919646222201", [WhatsAppNumber] = "919646222201" };
}
