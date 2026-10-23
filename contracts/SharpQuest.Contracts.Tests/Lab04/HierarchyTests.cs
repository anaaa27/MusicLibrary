namespace SharpQuest.Contracts.Tests.Lab04;

/// <summary>Lab 04: the shape of the family — an abstract base and two or more kinds that override it.</summary>
public class HierarchyTests : Lab04TestBase
{
    [Fact]
    public void Wiring_class_can_be_created()
    {
        Assert.NotNull(Wiring);
    }

    [Fact]
    public void The_base_is_an_abstract_class()
    {
        var b = BaseType;

        Assert.True(b.IsClass, $"BaseType is {Describe(b)}, which is not a class. Lab 04 wants an abstract class.");
        Assert.True(b.IsAbstract, $"{Describe(b)} is not abstract. Mark it 'abstract' so nobody can create one directly.");
        Assert.False(b.IsInterface, $"{Describe(b)} is an interface. You already have interfaces from Lab 02; this is about inheritance.");
    }

    [Fact]
    public void The_base_is_an_item()
    {
        Assert.True(typeof(IItem).IsAssignableFrom(BaseType),
            $"{Describe(BaseType)} does not implement IItem, so your store can't hold it.");
    }

    [Fact]
    public void There_are_at_least_two_kinds_and_they_are_different_types()
    {
        var kinds = Kinds;

        Assert.True(kinds.Distinct().Count() == kinds.Count,
            "KindTypes contains the same type twice. Two different kinds, please.");
    }

    [Fact]
    public void Every_kind_is_a_concrete_type_derived_from_the_base()
    {
        foreach (var kind in Kinds)
        {
            Assert.True(kind != BaseType, $"KindTypes contains the base itself ({Describe(kind)}). List the derived types.");
            Assert.True(BaseType.IsAssignableFrom(kind), $"{Describe(kind)} does not derive from {Describe(BaseType)}.");
            Assert.False(kind.IsAbstract, $"{Describe(kind)} is abstract, so nothing can be created from it.");
        }
    }

    [Fact]
    public void The_base_declares_something_for_the_kinds_to_override()
    {
        var members = PolymorphicMembers(BaseType);

        Assert.True(members.Count > 0,
            $"{Describe(BaseType)} declares no abstract or virtual member that takes no parameters and returns a string. " +
            "Add one — a method like 'public abstract string Describe();' or a property — for each kind to override.");
    }

    [Fact]
    public void Every_kind_overrides_it()
    {
        var members = PolymorphicMembers(BaseType);

        foreach (var kind in Kinds)
        {
            var overridden = members.Any(m => OverrideIn(kind, m) is not null);
            Assert.True(overridden,
                $"{Describe(kind)} overrides none of the base's polymorphic members " +
                $"({string.Join(", ", members.Select(m => m.Name))}). Without an override, the two kinds behave identically.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void CreateOfKind_returns_that_kind(int kind)
    {
        var item = Sample(kind, 7);

        Assert.True(Kinds[kind].IsInstanceOfType(item),
            $"CreateOfKind({kind}, 7) returned {Describe(item.GetType())}, but KindTypes[{kind}] is {Describe(Kinds[kind])}.");
    }

    [Fact]
    public void Ids_are_stable_for_a_kind_and_seed_and_different_otherwise()
    {
        Assert.Equal(Sample(0, 4).Id, Sample(0, 4).Id);

        var ids = new[] { Sample(0, 1).Id, Sample(0, 2).Id, Sample(1, 1).Id, Sample(1, 2).Id };

        Assert.True(ids.Distinct(StringComparer.Ordinal).Count() == ids.Length,
            $"Different kinds or seeds gave the same Id ({string.Join(", ", ids)}). A store can't hold duplicates.");
    }

    [Fact]
    public void Items_of_every_kind_go_into_your_store()
    {
        var store = StoreWith((0, 1), (1, 2), (0, 3));

        Assert.Equal(3, store.Count);
    }

    [Fact]
    public void The_hierarchy_is_your_own_code()
    {
        foreach (var type in Kinds.Append(BaseType))
            Assert.True(type.Assembly.GetName().Name == "Capstone.Core",
                $"{Describe(type)} lives in {type.Assembly.GetName().Name}. Your hierarchy belongs in Capstone.Core, not in the contracts.");
    }

    [Fact]
    public void Nothing_about_the_base_hides_the_item_contract()
    {
        var item = Sample(0, 9);

        Assert.False(string.IsNullOrWhiteSpace(item.Id), "Items of your kinds must still have an Id (Lab 02's rule).");
        Assert.False(string.IsNullOrWhiteSpace(item.DisplayName), "Items of your kinds must still have a DisplayName.");
    }

    [Fact]
    public void The_polymorphic_member_is_not_just_the_display_name()
    {
        var members = PolymorphicMembers(BaseType);
        var item = Sample(0, 11);

        var values = members.Select(m => (string?)m.Invoke(item, null)).Where(v => v is not null).ToList();

        Assert.True(values.Count > 0, "The base's polymorphic member returned nothing for an item of your first kind.");
        Assert.Contains(values, v => !string.IsNullOrWhiteSpace(v));
    }
}
