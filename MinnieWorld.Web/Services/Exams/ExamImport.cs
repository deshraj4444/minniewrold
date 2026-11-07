using System.Text.Json;
using MinnieWorld.Web.Models;

namespace MinnieWorld.Web.Services.Exams;

/// <summary>
/// Reads questions from JSON written by people or other tools, so admins can paste a question bank as-is.
/// Accepts an array, or an object with a "questions" array. For each question:
/// text: "text" | "question" | "q";
/// options: "options" array of 4, or "a".."d" / "option_a".."option_d" / "optionA".."optionD";
/// answer: "answer" | "correct" | "answer_index" | "correct_option" as a number 0–3 (1–4 when "oneBased": true),
/// a letter "A"–"D", a digit string "1"–"4", or the option text;
/// "explanation" | "solution" | "reason"; "section" | "subject" | "topic";
/// optional "marks" (default 1), "figure" (an SVG picture) and "passage"/"passageHi" (reading passage).
/// </summary>
public static class ExamImport
{
    public record Result(List<Question> Questions, List<string> Errors);

    public static Result Parse(string json)
    {
        var questions = new List<Question>();
        var errors = new List<string>();

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json, new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
        }
        catch (JsonException e)
        {
            errors.Add($"Not valid JSON: {e.Message}");
            return new Result(questions, errors);
        }

        using (doc)
        {
            var root = doc.RootElement;
            var items = root.ValueKind == JsonValueKind.Array ? root
                : root.ValueKind == JsonValueKind.Object && TryGet(root, out var q, "questions", "items", "data") && q.ValueKind == JsonValueKind.Array ? q
                : default;

            if (items.ValueKind != JsonValueKind.Array)
            {
                errors.Add("Expected a JSON array of questions, or an object with a \"questions\" array.");
                return new Result(questions, errors);
            }

            var n = 0;
            foreach (var item in items.EnumerateArray())
            {
                n++;
                if (TryRead(item, out var question, out var error))
                {
                    questions.Add(question!);
                }
                else
                {
                    errors.Add($"Question {n}: {error}");
                }
            }
        }

        return new Result(questions, errors);
    }

    private static bool TryRead(JsonElement item, out Question? question, out string error)
    {
        question = null;
        error = "";
        if (item.ValueKind != JsonValueKind.Object)
        {
            error = "not an object.";
            return false;
        }

        var text = Str(item, "text", "question", "q", "question_text");
        if (string.IsNullOrWhiteSpace(text))
        {
            error = "missing question text.";
            return false;
        }

        string[] options;
        if (TryGet(item, out var opts, "options", "choices") && opts.ValueKind == JsonValueKind.Array)
        {
            options = opts.EnumerateArray().Select(o => o.ValueKind == JsonValueKind.String ? o.GetString()!.Trim() : o.ToString().Trim()).ToArray();
        }
        else if (TryGet(item, out var optObj, "options", "choices") && optObj.ValueKind == JsonValueKind.Object)
        {
            options = new[] { "A", "B", "C", "D" }.Select(k => Str(optObj, k, k.ToLowerInvariant())).ToArray();
        }
        else
        {
            options = new[] { "a", "b", "c", "d" }.Select(k => Str(item, k, $"option_{k}", $"option{k.ToUpperInvariant()}", $"option{k}", k.ToUpperInvariant())).ToArray();
        }

        // Some sources prefix options with "(A) " or "A. "; strip that so it isn't shown twice.
        options = options.Select(StripLabel).ToArray();
        if (options.Length != 4 || options.Any(string.IsNullOrWhiteSpace))
        {
            error = "needs exactly 4 non-empty options.";
            return false;
        }

        var oneBased = TryGet(item, out var ob, "oneBased", "one_based") && ob.ValueKind == JsonValueKind.True;
        if (!TryGet(item, out var ans, "answer", "correct", "answer_index", "correct_option", "correctAnswer", "correct_answer", "ans"))
        {
            error = "missing answer.";
            return false;
        }

        var answer = ResolveAnswer(ans, options, oneBased);
        if (answer is null)
        {
            error = $"can't tell which option the answer \"{ans}\" means.";
            return false;
        }

        // Optional Hindi version: "textHi"/"text_hi"/"questionHi" and an "optionsHi" array of 4.
        var optionsHi = TryGet(item, out var oh, "optionsHi", "options_hi", "choicesHi") && oh.ValueKind == JsonValueKind.Array
            ? oh.EnumerateArray().Select(o => StripLabel(o.ValueKind == JsonValueKind.String ? o.GetString()! : o.ToString())).ToArray()
            : [];
        if (optionsHi.Length != 4) optionsHi = ["", "", "", ""];

        question = new Question
        {
            TextHi = Str(item, "textHi", "text_hi", "questionHi", "question_hi"),
            OptionAHi = Trunc(optionsHi[0], 1000),
            OptionBHi = Trunc(optionsHi[1], 1000),
            OptionCHi = Trunc(optionsHi[2], 1000),
            OptionDHi = Trunc(optionsHi[3], 1000),
            ExplanationHi = Trunc(Str(item, "explanationHi", "explanation_hi", "solutionHi"), 2000),
            Section = Trunc(Str(item, "section", "subject", "topic", "category"), 100),
            Text = text.Trim(),
            OptionA = Trunc(options[0], 1000),
            OptionB = Trunc(options[1], 1000),
            OptionC = Trunc(options[2], 1000),
            OptionD = Trunc(options[3], 1000),
            Answer = answer.Value,
            Explanation = Trunc(Str(item, "explanation", "solution", "reason", "explain"), 2000),
            Figure = Str(item, "figure", "svg"),
            Passage = Str(item, "passage", "paragraph"),
            PassageHi = Str(item, "passageHi", "passage_hi"),
            Marks = TryGet(item, out var mk, "marks", "mark") && mk.ValueKind == JsonValueKind.Number && mk.TryGetDecimal(out var m) && m is > 0 and <= 10 ? m : 1
        };
        return true;
    }

    private static int? ResolveAnswer(JsonElement ans, string[] options, bool oneBased)
    {
        if (ans.ValueKind == JsonValueKind.Number && ans.TryGetInt32(out var i))
        {
            var idx = oneBased ? i - 1 : i;
            return idx is >= 0 and <= 3 ? idx : null;
        }

        if (ans.ValueKind != JsonValueKind.String) return null;
        var s = ans.GetString()!.Trim();
        var bare = s.Trim('(', ')', '.', ' ').ToUpperInvariant();
        if (bare.Length == 1 && bare[0] is >= 'A' and <= 'D') return bare[0] - 'A';
        // A digit typed as text ("2") is how people number options, so it's read as 1-based; "0" can only mean the first.
        if (bare.Length == 1 && bare[0] is >= '0' and <= '4') return bare[0] == '0' ? 0 : bare[0] - '1';

        var match = Array.FindIndex(options, o => string.Equals(o, StripLabel(s), StringComparison.OrdinalIgnoreCase));
        return match >= 0 ? match : null;
    }

    private static string StripLabel(string option)
    {
        var o = option.Trim();
        if (o.Length > 3 && o[0] == '(' && o[2] == ')' && char.ToUpperInvariant(o[1]) is >= 'A' and <= 'D') return o[3..].Trim();
        if (o.Length > 2 && char.ToUpperInvariant(o[0]) is >= 'A' and <= 'D' && o[1] is '.' or ')') return o[2..].Trim();
        return o;
    }

    private static bool TryGet(JsonElement obj, out JsonElement value, params string[] names)
    {
        foreach (var prop in obj.EnumerateObject())
        {
            if (names.Any(n => string.Equals(prop.Name, n, StringComparison.OrdinalIgnoreCase)) && prop.Value.ValueKind != JsonValueKind.Null)
            {
                value = prop.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string Str(JsonElement obj, params string[] names) =>
        TryGet(obj, out var v, names) ? (v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : v.ToString()).Trim() : "";

    private static string Trunc(string s, int max) => s.Length <= max ? s : s[..max];
}
