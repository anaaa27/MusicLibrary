using System.Text.RegularExpressions;
using SharpQuest.Contracts.Tests.Support;

namespace SharpQuest.Contracts.Tests.Lab03;

/// <summary>Lab 03: <see cref="ILab03Wiring.Search"/> over your own store.</summary>
public class SearchTests
{
    private ILab03Wiring? _wiringCache;
    private ILab03Wiring Wiring => _wiringCache ??= StudentCode.Wiring<ILab03Wiring>();

    private IItemStore StoreWith(params int[] seeds)
    {
        var store = Wiring.CreateStore();
        foreach (var seed in seeds) store.Add(Wiring.CreateSample(seed));
        return store;
    }

    private static string[] WordsOf(string displayName) =>
        Regex.Matches(displayName, @"\p{L}{2,}").Select(m => m.Value).ToArray();

    [Fact]
    public void Wiring_class_can_be_created()
    {
        Assert.NotNull(Wiring);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void An_empty_query_returns_everything_in_store_order(string query)
    {
        var store = StoreWith(5, 1, 3);

        var ids = Wiring.Search(store, query).Select(i => i.Id);

        Assert.Equal(store.All().Select(i => i.Id), ids);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void A_shouted_word_from_a_display_name_finds_that_item(int seed)
    {
        var store = StoreWith(1, 2, 3);
        var item = Wiring.CreateSample(seed);
        var words = WordsOf(item.DisplayName);
        Assert.True(words.Length > 0, $"DisplayName '{item.DisplayName}' has no word of two or more letters to search for.");

        var query = words.OrderByDescending(w => w.Length).First().ToUpperInvariant();
        var found = Wiring.Search(store, query).Select(i => i.Id).ToList();

        Assert.True(found.Contains(item.Id),
            $"Searching for '{query}' didn't find '{item.DisplayName}'. Matching must ignore case (rule R2).");
    }

    [Fact]
    public void Accents_in_the_query_are_ignored()
    {
        var store = StoreWith(1);
        var item = Wiring.CreateSample(1);
        var word = WordsOf(item.DisplayName).OrderByDescending(w => w.Length).FirstOrDefault();
        if (word is null) return;
        var vowel = word.IndexOfAny("aeiouAEIOU".ToCharArray());
        if (vowel < 0) return;   // nothing to accent

        var accented = word[..vowel] + Accent(word[vowel]) + word[(vowel + 1)..];
        var found = Wiring.Search(store, accented).Select(i => i.Id);

        Assert.True(found.Contains(item.Id),
            $"Searching for '{accented}' didn't find '{item.DisplayName}'. Diacritics must be ignored (rule R3).");
    }

    [Fact]
    public void Word_order_in_the_query_does_not_matter()
    {
        var store = StoreWith(1, 2, 3, 4);
        var item = Wiring.CreateSample(2);
        var words = WordsOf(item.DisplayName);
        if (words.Length < 2) return;   // a one-word name can't be reordered

        var reversed = string.Join(" ", words.Reverse());
        var found = Wiring.Search(store, reversed).Select(i => i.Id);

        Assert.True(found.Contains(item.Id),
            $"Searching for '{reversed}' didn't find '{item.DisplayName}'. Word order must not matter (rule R5).");
    }

    [Fact]
    public void A_nonsense_query_finds_nothing()
    {
        var store = StoreWith(1, 2, 3);

        Assert.Empty(Wiring.Search(store, "zqxjv-no-such-thing"));
    }

    [Fact]
    public void All_words_must_match()
    {
        var store = StoreWith(1, 2, 3);
        var item = Wiring.CreateSample(1);
        var word = WordsOf(item.DisplayName).First();

        var found = Wiring.Search(store, word + " zqxjv").Select(i => i.Id);

        Assert.False(found.Contains(item.Id),
            $"'{word} zqxjv' found '{item.DisplayName}', but 'zqxjv' isn't in it. Every word must match (rule R4).");
    }

    [Fact]
    public void Searching_does_not_change_the_store()
    {
        var store = StoreWith(1, 2, 3);
        var before = store.All().Select(i => i.Id).ToList();

        Wiring.Search(store, "a");
        Wiring.Search(store, "");

        Assert.Equal(before, store.All().Select(i => i.Id));
    }

    private static string Accent(char c) => c switch
    {
        'a' => "á", 'e' => "é", 'i' => "í", 'o' => "ó", 'u' => "ú",
        'A' => "Á", 'E' => "É", 'I' => "Í", 'O' => "Ó", 'U' => "Ú",
        _ => c.ToString(),
    };
}
