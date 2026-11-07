using System.Text.Json;
using MinnieWorld.Web.Endpoints;
using MinnieWorld.Web.Services.Cbse;
using MinnieWorld.Web.Services.Seo;

namespace MinnieWorld.Tests;

public class CbseValidatorTests
{
    private static readonly string[] Subjects = CbseCatalog.Subjects.Select(s => s.Key).ToArray();

    private static CbseChapter Valid(params string[] gamesJson) => new()
    {
        Class = 6, Subject = "mathematics", Number = 1, Slug = "sample-chapter", Title = "Sample", Summary = "A summary.", Notes = "## Notes", Updated = "2026-11-07",
        Games = gamesJson.Select(j => JsonDocument.Parse(j).RootElement.Clone()).ToList(),
        Questions = [new("case", "A case.", null, "Q?", null, ["a", "b", "c", "d"], null, 2, "Because.", null)],
        Faqs = [new("Q?", null, "A.", null)]
    };

    [Fact]
    public void AValidChapterWithEveryGameTypeHasNoErrors()
    {
        var chapter = Valid(
            """{"id":"sort","type":"dragSort","title":"Sort","buckets":[{"id":"a","label":"A"},{"id":"b","label":"B"}],"items":[{"text":"1","bucket":"a"},{"text":"2","bucket":"b"}]}""",
            """{"id":"match","type":"matchPairs","title":"Match","pairs":[{"left":"x","right":"1"},{"left":"y","right":"2"}]}""",
            """{"id":"order","type":"timeline","title":"Order","items":[{"text":"1"},{"text":"2"},{"text":"3"}]}""",
            """{"id":"label","type":"labelDiagram","title":"Label","image":"<svg/>","hotspots":[{"label":"a","x":10,"y":20},{"label":"b","x":50,"y":60}]}""",
            """{"id":"line","type":"numberLine","title":"Line","min":-2,"max":2,"step":0.25,"format":"fraction","rounds":[{"prompt":"Put 3/4","value":0.75}]}""",
            """{"id":"quiz","type":"speedQuiz","title":"Quiz","questions":[{"text":"1","options":["a","b"],"answer":0},{"text":"2","options":["a","b"],"answer":1},{"text":"3","options":["a","b"],"answer":0},{"text":"4","options":["a","b"],"answer":1},{"text":"5","options":["a","b"],"answer":0}]}""");

        Assert.Empty(CbseValidator.Validate(chapter, Subjects));
    }

    [Fact]
    public void ReportsBadChapterFields()
    {
        var chapter = Valid() with { Class = 9, Subject = "science", Slug = "Bad Slug", Updated = "07-11-2026" };
        var errors = CbseValidator.Validate(chapter, Subjects);

        Assert.Contains(errors, e => e.Contains("class must be one of 6"));
        Assert.Contains(errors, e => e.Contains("unknown subject"));
        Assert.Contains(errors, e => e.Contains("slug"));
        Assert.Contains(errors, e => e.Contains("updated"));
    }

    [Fact]
    public void ACaseQuestionNeedsItsCase()
    {
        var chapter = Valid() with { Questions = [new("case", null, null, "Q?", null, ["a", "b", "c", "d"], null, 0, "Why.", null)] };
        Assert.Contains(CbseValidator.Validate(chapter, Subjects), e => e.Contains("case is required"));
    }

    [Fact]
    public void ReportsBrokenGames()
    {
        var chapter = Valid(
            """{"id":"sort","type":"dragSort","title":"Sort","buckets":[{"id":"a","label":"A"},{"id":"b","label":"B"}],"items":[{"text":"1","bucket":"zzz"},{"text":"2","bucket":"b"}]}""",
            """{"id":"line","type":"numberLine","title":"Line","min":0,"max":1,"step":0.1,"rounds":[{"prompt":"Put 5","value":5}]}""",
            """{"id":"line","type":"spinWheel","title":"Dup"}""");
        var errors = CbseValidator.Validate(chapter, Subjects);

        Assert.Contains(errors, e => e.Contains("unknown bucket"));
        Assert.Contains(errors, e => e.Contains("between min and max"));
        Assert.Contains(errors, e => e.Contains("used twice"));
        Assert.Contains(errors, e => e.Contains("type must be one of"));
    }

    [Theory]
    [InlineData("class-6", 6)]
    [InlineData("class-9", null)]
    [InlineData("class-11", null)]
    [InlineData("class-06", null)]
    [InlineData("grade-9", null)]
    public void ParsesClassSegments(string segment, int? expected) => Assert.Equal(expected, CbseCatalog.ParseClass(segment));
}

public class SeoRedirectTests
{
    [Theory]
    [InlineData("/gk/jee", "/jee")]
    [InlineData("/gk/exams/jee-main", "/jee")]
    [InlineData("/jee/", "/jee")]
    [InlineData("/gk/tests/jee-main-physics-kinematics-chapter-test", "/jee/physics/kinematics-test")]
    [InlineData("/JEE/Physics/Kinematics-Notes", "/jee/physics/kinematics-notes")]
    [InlineData("/jee", null)]
    [InlineData("/gk/tests/hp-tet-jbt-mock-1", null)]
    public void JeeRedirects(string path, string? target) => Assert.Equal(target, JeePages.RedirectFor(path));

    [Theory]
    [InlineData("/CBSE/Class-6/", "/cbse/class-6")]
    [InlineData("/cbse/", "/cbse/class-6")]
    [InlineData("/cbse", "/cbse/class-6")]
    [InlineData("/cbse/class-6/personality-development", null)]
    [InlineData("/cbsecourses", null)]
    [InlineData("/gk", null)]
    public void CbseRedirects(string path, string? target) => Assert.Equal(target, SeoEndpoints.CbseRedirectFor(path));

    [Theory]
    [InlineData("https://g.page/r/abc123/review", true)]
    [InlineData("https://search.google.com/local/writereview?placeid=XYZ", true)]
    [InlineData("https://www.google.co.in/maps/place/x", true)]
    [InlineData("https://maps.app.goo.gl/abc", true)]
    [InlineData("http://g.page/r/abc/review", false)]
    [InlineData("https://google.com.evil.example/review", false)]
    [InlineData("https://evil.example/?u=https://g.page", false)]
    [InlineData("", false)]
    public void OnlyGoogleReviewLinks(string url, bool ok) => Assert.Equal(ok, PublicEndpoints.IsGoogleLink(url));
}
