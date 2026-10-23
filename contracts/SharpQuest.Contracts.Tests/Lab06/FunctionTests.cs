namespace SharpQuest.Contracts.Tests.Lab06;

/// <summary>Lab 06: functions as parameters — the caller brings the rule, your code runs it.</summary>
public class FunctionTests : Lab06TestBase
{
    [Fact]
    public void Where_keeps_what_the_function_says_yes_to()
    {
        var store = StoreWith(1, 2, 3, 4);
        var wanted = store.All()[2].Id;

        var kept = Wiring.Where(store, item => item.Id == wanted);

        Assert.Equal(wanted, Assert.Single(kept).Id);
    }

    [Fact]
    public void A_function_that_always_says_no_keeps_nothing()
    {
        Assert.Empty(Wiring.Where(StoreWith(1, 2, 3), _ => false));
    }

    [Fact]
    public void A_function_that_always_says_yes_keeps_everything_in_store_order()
    {
        var store = StoreWith(5, 1, 9);

        var kept = Wiring.Where(store, _ => true);

        Assert.Equal(string.Join(",", store.All().Select(i => i.Id)), string.Join(",", kept.Select(i => i.Id)));
    }

    [Fact]
    public void The_function_is_asked_about_every_item_exactly_once()
    {
        var store = StoreWith(1, 2, 3, 4, 5);
        var asked = 0;

        Wiring.Where(store, _ => { asked++; return true; });

        Assert.True(asked == 5, $"The predicate was called {asked} times for 5 items. Call the caller's function once per item.");
    }

    [Fact]
    public void Where_on_an_empty_store_asks_nothing()
    {
        var asked = 0;

        var kept = Wiring.Where(Wiring.CreateStore(), _ => { asked++; return true; });

        Assert.Empty(kept);
        Assert.Equal(0, asked);
    }

    [Fact]
    public void Where_does_not_change_the_store()
    {
        var store = StoreWith(1, 2, 3);

        Wiring.Where(store, _ => false);

        Assert.Equal(3, store.Count);
    }

    [Fact]
    public void Map_uses_the_function_for_every_item_in_order()
    {
        var store = StoreWith(2, 8, 4);

        var described = string.Join(",", Wiring.Map(store, item => $"<{item.Id}>"));
        var expected = string.Join(",", store.All().Select(i => $"<{i.Id}>"));

        Assert.True(described == expected,
            $"Map returned \"{described}\" but the store holds \"{expected}\". " +
            "One string per item, in store order — don't sort or filter the caller's results.");
    }

    [Fact]
    public void Map_can_return_the_same_text_for_everything()
    {
        var store = StoreWith(1, 2);

        Assert.Equal("x,x", string.Join(",", Wiring.Map(store, _ => "x")));
    }

    [Fact]
    public void Map_on_an_empty_store_returns_nothing()
    {
        Assert.Empty(Wiring.Map(Wiring.CreateStore(), item => item.DisplayName));
    }

    [Fact]
    public void ForEach_runs_the_action_on_every_item_in_order()
    {
        var store = StoreWith(3, 6, 9);
        var seen = new List<string>();

        Wiring.ForEach(store, item => seen.Add(item.Id));

        Assert.Equal(string.Join(",", store.All().Select(i => i.Id)), string.Join(",", seen));
    }

    [Fact]
    public void ForEach_on_an_empty_store_does_nothing()
    {
        var ran = false;

        Wiring.ForEach(Wiring.CreateStore(), _ => ran = true);

        Assert.False(ran);
    }

    [Fact]
    public void A_function_can_carry_state_from_where_it_was_written()
    {
        var store = StoreWith(1, 2, 3);
        var total = 0;

        Wiring.ForEach(store, item => total += item.DisplayName.Length);

        Assert.True(total == store.All().Sum(i => i.DisplayName.Length),
            "The action's captured variable didn't add up. A lambda keeps a live link to the variables around it.");
    }

    [Fact]
    public void A_missing_predicate_is_a_programming_error()
    {
        var store = StoreWith(1);

        var ex = Record.Exception(() => Wiring.Where(store, null!));

        Assert.True(ex is not null, "Where(store, null) returned instead of throwing.");
        Assert.IsAssignableFrom<ArgumentNullException>(ex);
    }

    [Fact]
    public void A_missing_map_function_is_a_programming_error()
    {
        var store = StoreWith(1);

        var ex = Record.Exception(() => Wiring.Map(store, null!));

        Assert.True(ex is not null, "Map(store, null) returned instead of throwing.");
        Assert.IsAssignableFrom<ArgumentNullException>(ex);
    }

    [Fact]
    public void A_missing_action_is_a_programming_error()
    {
        var store = StoreWith(1);

        var ex = Record.Exception(() => Wiring.ForEach(store, null!));

        Assert.True(ex is not null, "ForEach(store, null) returned instead of throwing.");
        Assert.IsAssignableFrom<ArgumentNullException>(ex);
    }
}
