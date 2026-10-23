namespace SharpQuest.Contracts.Tests.Lab04;

/// <summary>Lab 04: the two parameter keywords worth knowing — <c>out</c> in a Try method, and <c>params</c>.</summary>
public class ParameterTests : Lab04TestBase
{
    [Fact]
    public void A_stored_item_is_found_by_its_display_name()
    {
        var store = StoreWith((0, 1), (1, 2));
        var wanted = store.All()[1];

        var ok = Wiring.TryFindByName(store, wanted.DisplayName, out var found);

        Assert.True(ok, $"TryFindByName said false for \"{wanted.DisplayName}\", which is in the store.");
        Assert.NotNull(found);
        Assert.Equal(wanted.Id, found!.Id);
    }

    [Fact]
    public void A_name_nobody_has_is_not_found_and_gives_null()
    {
        var store = StoreWith((0, 1));

        var ok = Wiring.TryFindByName(store, "no item is called this", out var found);

        Assert.False(ok, "TryFindByName said true for a name that isn't in the store.");
        Assert.True(found is null, "When the answer is false, the out parameter must be null — not a leftover item.");
    }

    [Fact]
    public void An_empty_store_finds_nothing()
    {
        var ok = Wiring.TryFindByName(Wiring.CreateStore(), "anything", out var found);

        Assert.False(ok);
        Assert.True(found is null);
    }

    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_missing_name_is_simply_not_found(string? name)
    {
        var store = StoreWith((0, 1), (1, 2));

        var ex = Record.Exception(() => Wiring.TryFindByName(store, name!, out _));

        Assert.True(ex is null,
            $"TryFindByName threw {ex?.GetType().Name} for {(name is null ? "a null" : "a blank")} name. " +
            "A Try method answers false; it doesn't throw.");

        var ok = Wiring.TryFindByName(store, name!, out var found);

        Assert.False(ok, $"TryFindByName said true for {(name is null ? "null" : $"\"{name}\"")}.");
        Assert.True(found is null, "When the answer is false, the out parameter must be null.");
    }

    [Fact]
    public void Searching_by_name_does_not_change_the_store()
    {
        var store = StoreWith((0, 1), (1, 2));

        Wiring.TryFindByName(store, store.All()[0].DisplayName, out _);

        Assert.Equal(2, store.Count);
    }

    [Fact]
    public void AddAll_with_nothing_to_add_adds_nothing()
    {
        var store = Wiring.CreateStore();

        var added = Wiring.AddAll(store);

        Assert.Equal(0, added);
        Assert.Equal(0, store.Count);
    }

    [Fact]
    public void AddAll_adds_every_item_in_order()
    {
        var store = Wiring.CreateStore();
        var items = new[] { Sample(0, 1), Sample(1, 2), Sample(0, 3) };

        var added = Wiring.AddAll(store, items[0], items[1], items[2]);

        Assert.Equal(3, added);
        Assert.Equal(items.Select(i => i.Id), store.All().Select(i => i.Id));
    }

    [Fact]
    public void AddAll_takes_a_single_item_too()
    {
        var store = Wiring.CreateStore();

        Assert.Equal(1, Wiring.AddAll(store, Sample(1, 9)));
        Assert.Equal(1, store.Count);
    }

    [Fact]
    public void A_duplicate_inside_the_batch_throws_and_keeps_what_came_before()
    {
        var store = Wiring.CreateStore();
        var first = Sample(0, 1);
        var duplicate = Sample(0, 1);

        var ex = Record.Exception(() => Wiring.AddAll(store, first, duplicate));

        Assert.True(ex is not null, "Adding two items with the same Id should throw — that's still your store's rule from Lab 02.");
        Assert.IsAssignableFrom<ArgumentException>(ex);
        Assert.Equal(1, store.Count);
    }

    [Fact]
    public void A_null_item_in_the_batch_throws()
    {
        var store = Wiring.CreateStore();

        var ex = Record.Exception(() => Wiring.AddAll(store, Sample(0, 2), null!));

        Assert.True(ex is not null, "A null item should throw, as it does in Lab 02's Add.");
        Assert.IsAssignableFrom<ArgumentNullException>(ex);
    }
}
