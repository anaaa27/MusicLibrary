namespace SharpQuest.Contracts.Tests.Lab04;

/// <summary>Lab 04: the override actually decides, and one loop covers every kind.</summary>
public class PolymorphismTests : Lab04TestBase
{
    [Fact]
    public void Describe_says_something()
    {
        var text = Wiring.Describe(Sample(0, 1));

        Assert.False(string.IsNullOrWhiteSpace(text), "Describe returned nothing for an item of your first kind.");
    }

    [Fact]
    public void Two_kinds_from_the_same_seed_describe_themselves_differently()
    {
        var wild = Sample(0, 5);
        var other = Sample(1, 5);

        // Ignore the parts that differ anyway (the Id and the name): what's left is what the overrides added.
        var first = WithoutIdentity(wild, Wiring.Describe(wild));
        var second = WithoutIdentity(other, Wiring.Describe(other));

        Assert.True(first != second,
            $"Take the Id and the name out of both descriptions and they say exactly the same thing (\"{first}\"). " +
            "Each override has to add something of its own, or the hierarchy buys you nothing.");
    }

    private static string WithoutIdentity(IItem item, string description) =>
        description.Replace(item.Id, "", StringComparison.Ordinal)
                   .Replace(item.DisplayName, "", StringComparison.Ordinal)
                   .Trim();

    [Fact]
    public void Describe_returns_what_the_override_returns()
    {
        var item = Sample(1, 3);
        var members = PolymorphicMembers(BaseType);

        var fromOverride = members
            .Select(m => (string?)m.Invoke(item, null))
            .Where(v => v is not null)
            .ToList();

        Assert.Contains(Wiring.Describe(item), fromOverride);
    }

    [Fact]
    public void Describing_the_same_item_twice_gives_the_same_answer()
    {
        var item = Sample(0, 8);

        Assert.Equal(Wiring.Describe(item), Wiring.Describe(item));
    }

    [Fact]
    public void An_empty_store_describes_nothing()
    {
        Assert.Empty(Wiring.DescribeAll(Wiring.CreateStore()));
    }

    [Fact]
    public void DescribeAll_covers_every_item_in_store_order()
    {
        var store = StoreWith((0, 1), (1, 2), (0, 3), (1, 4));

        var described = Wiring.DescribeAll(store);
        var expected = store.All().Select(Wiring.Describe).ToList();

        Assert.True(described.Count == store.Count,
            $"The store holds {store.Count} items but DescribeAll returned {described.Count} lines.");

        for (var i = 0; i < expected.Count; i++)
            Assert.True(described[i] == expected[i],
                $"Line {i} is \"{described[i]}\" but item {i} in the store describes itself as \"{expected[i]}\". " +
                "Keep store order and let each item speak for itself.");
    }

    [Fact]
    public void DescribeAll_does_not_change_the_store()
    {
        var store = StoreWith((0, 1), (1, 2));
        var before = store.All().Select(i => i.Id).ToList();

        Wiring.DescribeAll(store);

        Assert.Equal(before, store.All().Select(i => i.Id));
        Assert.Equal(2, store.Count);
    }

    [Fact]
    public void A_store_of_one_kind_still_works()
    {
        var store = StoreWith((1, 6), (1, 7));

        var described = Wiring.DescribeAll(store);

        Assert.Equal(2, described.Count);
        Assert.All(described, d => Assert.False(string.IsNullOrWhiteSpace(d)));
    }
}
