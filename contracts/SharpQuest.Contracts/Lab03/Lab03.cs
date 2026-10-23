// Lab 03 — Testing & the contract
// Two things live here:
//   1. SearchMatcher: a small, fully specified search rule. YOU write the tests for it.
//      The grader then runs your tests against eight subtly broken versions ("mutants").
//      Good tests catch them; weak tests don't.
//   2. ILab03Wiring: search over your own store, using SearchMatcher.

using System.Globalization;
using System.Text;

namespace SharpQuest.Contracts;

/// <summary>
/// Decides whether a text (usually an item's DisplayName) matches a search query.
/// The rules below are the whole specification. Write at least one test per rule.
/// <list type="number">
/// <item><b>R1</b> An empty or whitespace-only query matches every non-null text.</item>
/// <item><b>R2</b> Matching ignores case: "PIKA" matches "Pikachu".</item>
/// <item><b>R3</b> Matching ignores diacritics, in both directions: "cafe" matches "Café", "șosea" matches "Sosea".</item>
/// <item><b>R4</b> The query is split into words on whitespace. EVERY word must occur in the text.</item>
/// <item><b>R5</b> Word order doesn't matter: "lizard fire" matches "fire lizard".</item>
/// <item><b>R6</b> A word may occur anywhere, even inside a longer word: "chu" matches "Pikachu".</item>
/// <item><b>R7</b> A null text never matches (the result is false, not an exception).</item>
/// <item><b>R8</b> A null query is a programming error: <see cref="ArgumentNullException"/>.</item>
/// </list>
/// </summary>
public sealed class SearchMatcher
{
    /// <summary>
    /// The matcher to use. Normally the correct one.
    /// While grading, the SQ_MUTANT environment variable swaps in a deliberately broken one.
    /// Don't read that variable in your tests: the grader checks for it.
    /// </summary>
    public static SearchMatcher Create() =>
        new(int.TryParse(Environment.GetEnvironmentVariable("SQ_MUTANT"), out var m) && m is >= 1 and <= MutantCount ? m : 0);

    /// <summary>How many broken versions the grader tries.</summary>
    public const int MutantCount = 8;

    private readonly int _mutant;

    private SearchMatcher(int mutant) => _mutant = mutant;

    public bool Matches(string? text, string query)
    {
        if (query is null)
        {
            if (_mutant == 8) return text is not null;                                  // M8: breaks R8
            throw new ArgumentNullException(nameof(query));
        }
        if (text is null)
        {
            if (_mutant == 7) throw new NullReferenceException();                        // M7: breaks R7
            return false;
        }

        var words = query.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return _mutant != 4;                                      // M4: breaks R1

        var haystack = Normalise(text);
        var needles = words.Select(Normalise).ToList();

        if (_mutant == 1)                                                                // M1: breaks R2
        {
            haystack = StripDiacritics(text);
            needles = words.Select(StripDiacritics).ToList();
        }
        if (_mutant == 2)                                                                // M2: breaks R3
        {
            haystack = text.ToLowerInvariant();
            needles = words.Select(w => w.ToLowerInvariant()).ToList();
        }

        if (_mutant == 3) return needles.Any(n => haystack.Contains(n, StringComparison.Ordinal));   // M3: breaks R4

        if (_mutant == 5)                                                                // M5: breaks R5
        {
            var from = 0;
            foreach (var n in needles)
            {
                var at = haystack.IndexOf(n, from, StringComparison.Ordinal);
                if (at < 0) return false;
                from = at + n.Length;
            }
            return true;
        }

        if (_mutant == 6)                                                                // M6: breaks R6
        {
            var textWords = haystack.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            return needles.All(n => textWords.Any(t => t.StartsWith(n, StringComparison.Ordinal)));
        }

        return needles.All(n => haystack.Contains(n, StringComparison.Ordinal));
    }

    private static string Normalise(string s) => StripDiacritics(s).ToLowerInvariant();

    private static string StripDiacritics(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (var c in s.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}

/// <summary>Tells the Lab 03 tests how to search your capstone. Implement once, in src/Capstone.Core/Wiring/.</summary>
public interface ILab03Wiring
{
    IItemStore CreateStore();

    /// <summary>A valid item; same contract as <see cref="ILab02Wiring.CreateSample"/>.</summary>
    IItem CreateSample(int seed);

    /// <summary>
    /// The items in <paramref name="store"/> whose DisplayName matches <paramref name="query"/>
    /// according to <see cref="SearchMatcher"/>, in store order. You may match more fields too.
    /// Searching never changes the store.
    /// </summary>
    IReadOnlyList<IItem> Search(IItemStore store, string query);
}
