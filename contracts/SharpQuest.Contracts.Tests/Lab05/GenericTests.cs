using System.Reflection;

namespace SharpQuest.Contracts.Tests.Lab05;

/// <summary>Lab 05: a generic type of your own, doing a real job — <see cref="ILab05Wiring.Recent{T}"/>.</summary>
public class GenericTests : Lab05TestBase
{
    private Type Definition =>
        Wiring.GenericDefinition ?? Fail<Type>("GenericDefinition is null. Return your own generic type, e.g. typeof(RecentlyUsed<>).");

    [Fact]
    public void The_generic_type_is_yours_and_open()
    {
        var d = Definition;

        Assert.True(d.Assembly.GetName().Name == "Capstone.Core",
            $"{Name(d)} lives in {d.Assembly.GetName().Name}. Write your own generic type in Capstone.Core.");
        Assert.True(d.IsGenericTypeDefinition,
            $"{Name(d)} is not an open generic type. Return typeof(YourType<>) — with the angle brackets empty — not a closed one like typeof(YourType<string>).");
    }

    [Fact]
    public void Its_type_parameter_is_actually_used()
    {
        var d = Definition;
        var parameters = d.GetGenericArguments();

        Assert.True(parameters.Length > 0, $"{Name(d)} has no type parameter.");

        var used = d.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Any(m => m switch
            {
                MethodInfo method => Mentions(method.ReturnType) || method.GetParameters().Any(p => Mentions(p.ParameterType)),
                PropertyInfo property => Mentions(property.PropertyType),
                FieldInfo field => Mentions(field.FieldType),
                ConstructorInfo ctor => ctor.GetParameters().Any(p => Mentions(p.ParameterType)),
                _ => false,
            });

        Assert.True(used,
            $"Nothing in {Name(d)} mentions its type parameter, so it isn't really generic. " +
            "A type parameter earns its place by appearing in a field, a parameter or a return type.");

        bool Mentions(Type t) =>
            t.IsGenericParameter || (t.ContainsGenericParameters && t.GetGenericArguments().Any(Mentions));
    }

    /// <summary>"a,b,c" pushed into a bag of this capacity, read back newest-first as one string.</summary>
    private string Pushed(int capacity, params string[] values) => string.Join(",", Wiring.Recent(capacity, values));

    [Fact]
    public void The_newest_value_comes_first()
    {
        Assert.Equal("c,b,a", Pushed(3, "a", "b", "c"));
    }

    [Fact]
    public void It_never_holds_more_than_its_capacity()
    {
        var held = string.Join(",", Wiring.Recent(2, 1, 2, 3, 4, 5));

        Assert.True(held == "5,4",
            $"After pushing 1,2,3,4,5 into a bag of capacity 2 it holds \"{held}\", not \"5,4\". " +
            "When it's full, the oldest value drops out.");
    }

    [Fact]
    public void Pushing_something_already_there_moves_it_to_the_front()
    {
        var held = Pushed(3, "a", "b", "a");

        Assert.True(held == "a,b",
            $"After pushing a, b, a into a bag of capacity 3 it holds \"{held}\", not \"a,b\". " +
            "A value already in there moves to the front; it doesn't take a second place.");
    }

    [Fact]
    public void The_same_value_over_and_over_is_still_one_value()
    {
        var held = Pushed(3, "a", "a", "a");

        Assert.True(held == "a", $"Pushing a three times left \"{held}\", not \"a\".");
    }

    [Fact]
    public void A_repeat_still_counts_as_the_newest()
    {
        Assert.Equal("a,c,b", Pushed(3, "a", "b", "c", "a"));
    }

    [Fact]
    public void Nothing_pushed_means_nothing_held()
    {
        Assert.Empty(Wiring.Recent<string>(3));
    }

    [Fact]
    public void Fewer_values_than_capacity_is_fine()
    {
        Assert.Equal("2,1", string.Join(",", Wiring.Recent(5, 1, 2)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void A_capacity_below_one_is_a_programming_error(int capacity)
    {
        var wiring = Wiring;

        var ex = Record.Exception(() => wiring.Recent(capacity, "a"));

        Assert.True(ex is not null, $"A capacity of {capacity} returned instead of throwing.");
        Assert.IsAssignableFrom<ArgumentOutOfRangeException>(ex);
    }

    [Fact]
    public void It_works_for_numbers_too()
    {
        Assert.Equal("3,1,2", string.Join(",", Wiring.Recent(3, 2, 1, 3)));
    }

    [Fact]
    public void It_works_for_your_own_items()
    {
        var first = Wiring.CreateSample(1);
        var second = Wiring.CreateSample(2);

        var recent = Wiring.Recent(2, first, second, first);

        Assert.Equal(2, recent.Count);
        Assert.Same(first, recent[0]);
        Assert.Same(second, recent[1]);
    }

    [Fact]
    public void Two_uses_do_not_share_state()
    {
        var one = string.Join(",", Wiring.Recent(2, "a", "b"));
        var two = string.Join(",", Wiring.Recent(2, "x"));

        Assert.Equal("b,a", one);
        Assert.Equal("x", two);
    }
}
