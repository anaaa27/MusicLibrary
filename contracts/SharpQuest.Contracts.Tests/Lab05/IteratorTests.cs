using System.Text.RegularExpressions;

namespace SharpQuest.Contracts.Tests.Lab05;

/// <summary>Lab 05: <see cref="ILab05Wiring.Matching"/> — an iterator, and a lazy one.</summary>
public class IteratorTests : Lab05TestBase
{
    private static string FirstWordOf(string displayName) =>
        Regex.Matches(displayName, @"\p{L}{2,}").Select(m => m.Value).FirstOrDefault() ?? displayName;

    [Fact]
    public void Everything_matches_the_empty_text()
    {
        var store = StoreWith(1, 2, 3);

        Assert.Equal(store.All().Select(i => i.Id), Wiring.Matching(store, "").Select(i => i.Id));
    }

    [Fact]
    public void A_word_from_a_display_name_finds_that_item()
    {
        var store = StoreWith(6, 2);
        var wanted = store.All()[0];
        var word = FirstWordOf(wanted.DisplayName);

        var found = Wiring.Matching(store, word).Select(i => i.Id).ToList();

        Assert.Contains(wanted.Id, found);
    }

    [Fact]
    public void Matching_ignores_case()
    {
        var store = StoreWith(6, 2);
        var wanted = store.All()[0];
        var shouted = FirstWordOf(wanted.DisplayName).ToUpperInvariant();

        Assert.Contains(wanted.Id, Wiring.Matching(store, shouted).Select(i => i.Id));
    }

    [Fact]
    public void Text_nobody_has_matches_nothing()
    {
        var store = StoreWith(1, 2, 3);

        Assert.Empty(Wiring.Matching(store, "qqzzxx-nothing-is-called-this"));
    }

    [Fact]
    public void Matching_keeps_store_order()
    {
        var store = StoreWith(4, 8, 1, 5);

        var ids = Wiring.Matching(store, "").Select(i => i.Id).ToList();

        Assert.Equal(store.All().Select(i => i.Id), ids);
    }

    [Fact]
    public void Nothing_happens_until_you_enumerate()
    {
        var store = StoreWith(1, 2);

        var matches = Wiring.Matching(store, "");   // asked for, not enumerated
        store.Add(Wiring.CreateSample(3));          // the store changes in between
        var ids = matches.Select(i => i.Id).ToList();

        Assert.True(ids.Count == 3,
            $"Enumerating after adding an item saw {ids.Count} items, not 3. " +
            "An iterator written with 'yield return' does its work while you enumerate it, so it sees the item added " +
            "in between. Building a List inside Matching and returning it is what gives 2.");
    }

    [Fact]
    public void Matching_does_not_change_the_store()
    {
        var store = StoreWith(1, 2, 3);

        foreach (var _ in Wiring.Matching(store, "")) { }

        Assert.Equal(3, store.Count);
    }

    [Fact]
    public void Matching_can_be_enumerated_more_than_once()
    {
        var store = StoreWith(1, 2);

        var matches = Wiring.Matching(store, "");

        Assert.Equal(matches.Select(i => i.Id), matches.Select(i => i.Id));
    }
}
